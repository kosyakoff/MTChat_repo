using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel;
using MTChat.Common;
using NLog;
using System.Threading;
using MTChat.Common.Messages;

namespace MTChat.WcfService
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession,
        ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class ChatService : IChatService
    {
        // таймаут бездействия пользователя
        private readonly TimeSpan _inactiveTimeout = TimeSpan.FromMinutes(10);
        private Timer _inactiveTimer;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly object SyncObj = new object();
        private static readonly Dictionary<Person, EventHandler<ChatEventArgs>> Persons = new Dictionary<Person, EventHandler<ChatEventArgs>>();
        // текущий пользователь
        private Person _person;
        private IChatServiceCallback _callback;

        public static event EventHandler<ChatEventArgs> ChatEvent;
        private EventHandler<ChatEventArgs> _eventHandler;


        #region IChatService members

        public OperationResult Say(TextMessage msg)
        {
            var e = new ChatEventArgs
            {
                CallbackType = CallbackType.Receive,
                Person = msg.From,
                Message = msg.Text
            };

            ResetTimer();
            Logger.Info("{0}: {1}", msg.From.Name, msg.Text);
            BroadcastMessage(e);

            return new OperationResult { Success = true };
        }

        public OperationResult Whisper(PersonalTextMessage msg)
        {
            var e = new ChatEventArgs
            {
                CallbackType = CallbackType.ReceiveWhisper,
                Person = msg.From,
                Message = msg.Text
            };
            var opResult = new OperationResult();


            EventHandler<ChatEventArgs> personTo;
            lock (SyncObj)
            {
                personTo = GetPersonHandler(msg.To.Name);
                if (personTo == null)
                {
                    opResult.Success = false;
                    var em = $"Пользователь с именем {msg.To.Name} не найден";
                    opResult.Errors.Add(new OperationError { ErrorMessage = em });

                    Logger.Error(em);
                    return opResult;
                }
            }

            ResetTimer();
            personTo.BeginInvoke(this, e, EndAsync, null);
            // предотвращаем повторное появление персонального сообщения у отправителя
            if (personTo != _eventHandler)
            {
                _eventHandler.BeginInvoke(this, e, EndAsync, null);
            }

            Logger.Info("{0} шепнул: {1}", msg.From.Name, msg.Text);

            opResult.Success = true;
            return opResult;
        }

        public OperationResult<Person[]> Join(Person person)
        {
            _eventHandler = EventHandler;
            var opResult = new OperationResult<Person[]>();

            lock (SyncObj)
            {
                if (!CheckIfPersonExists(person.Name))
                {
                    _person = person;
                    Persons.Add(person, EventHandler);
                }
                else
                {
                    opResult.Success = false;
                    var em = $"Пользователь с именен {person.Name} уже в чате";
                    opResult.Errors.Add(new OperationError { ErrorMessage = em });
                    Logger.Error(em);

                    return opResult;
                }
            }

            _callback = OperationContext.Current.GetCallbackChannel<IChatServiceCallback>();
            var e = new ChatEventArgs
            {
                CallbackType = CallbackType.UserEnter,
                Person = _person
            };

            _inactiveTimer = new Timer(InactiveTimerCallback, null, _inactiveTimeout, TimeSpan.FromMilliseconds(-1));
            Logger.Info("{0} зашел в чат", _person.Name);
            BroadcastMessage(e);
            ChatEvent += _eventHandler;

            opResult.Result = new Person[Persons.Count];
            lock (SyncObj)
            {
                Persons.Keys.CopyTo(opResult.Result, 0);
            }

            opResult.Success = true;
            return opResult;
        }

        public OperationResult Leave()
        {
            var opResult = new OperationResult();
            if (_person == null) return opResult;

            // остановка таймера неактивности
            _inactiveTimer.Change(TimeSpan.FromMilliseconds(-1), TimeSpan.FromMilliseconds(-1));

            var personToRemove = GetPersonHandler(_person.Name);

            lock (SyncObj)
            {
                Persons.Remove(_person);
            }

            ChatEvent -= personToRemove;

            var e = new ChatEventArgs
            {
                CallbackType = CallbackType.UserLeave,
                Person = _person
            };

            Logger.Info("{0} покинул чат", _person.Name);
            _person = null;
            BroadcastMessage(e);

            opResult.Success = true;
            return opResult;
        }

        #endregion

        /// <summary>
        /// Обработчик событий чата, вызывающий методы обратного вызова
        /// </summary>
        private void EventHandler(object sender, ChatEventArgs e)
        {
            try
            {
                switch (e.CallbackType)
                {
                    case CallbackType.Receive:
                        _callback.Receive(e.Person, e.Message);
                        break;
                    case CallbackType.ReceiveWhisper:
                        _callback.ReceiveWhisper(e.Person, e.Message);
                        break;
                    case CallbackType.UserEnter:
                        _callback.UserEnter(e.Person);
                        break;
                    case CallbackType.UserLeave:
                        _callback.UserLeave(e.Person);
                        break;
                    case CallbackType.DisconnectByTimeout:
                        _callback.DisconnectByTimeout();
                        break;
                }
            }
            catch
            {
                Leave();
            }
        }

        /// <summary>
        /// Асинронная отправка сообщения всем подключенным пользователям
        /// </summary>
        private void BroadcastMessage(ChatEventArgs e)
        {
            var temp = ChatEvent;

            if (temp != null)
            {
                foreach (var d in temp.GetInvocationList())
                {
                    var handler = (EventHandler<ChatEventArgs>)d;
                    handler.BeginInvoke(this, e, EndAsync, null);
                }
            }
        }

        /// <summary>
        /// Завершение асинхронной операции
        /// </summary>
        private void EndAsync(IAsyncResult ar)
        {
            EventHandler<ChatEventArgs> d = null;

            try
            {
                var asres = (System.Runtime.Remoting.Messaging.AsyncResult)ar;
                d = (EventHandler<ChatEventArgs>)asres.AsyncDelegate;
                d.EndInvoke(ar);
            }
            catch
            {
                ChatEvent -= d;
            }
        }

        /// <summary>
        /// Обратный вызов таймера неактивности
        /// </summary>
        private void InactiveTimerCallback(object state)
        {
            var e = new ChatEventArgs
            {
                CallbackType = CallbackType.DisconnectByTimeout,
                Person = _person
            };

            _eventHandler.BeginInvoke(this, e, EndAsync, null);
            Logger.Info("{0} отключен по таймауту", _person.Name);

            Leave();
        }

        #region Helpers

        private bool CheckIfPersonExists(string name)
        {
            return GetPersonByName(name) != null;
        }

        private EventHandler<ChatEventArgs> GetPersonHandler(string name)
        {
            EventHandler<ChatEventArgs> chatTo;
            Persons.TryGetValue(GetPersonByName(name), out chatTo);

            return chatTo;
        }

        private Person GetPersonByName(string name)
        {
            return Persons.Keys.FirstOrDefault(p => p.Name.Equals(name));
        }

        /// <summary>
        /// Перезапускаем таймер неактивности текущего пользователя
        /// </summary>
        private void ResetTimer()
        {
            _inactiveTimer.Change(_inactiveTimeout, TimeSpan.FromMilliseconds(-1));
        }

        #endregion
    }
}
