using System.Runtime.Serialization;

namespace MTChat.Common
{
    [DataContract]
    public class Person
    {
        [DataMember]
        public string Name { get; set; }

        public override bool Equals(object obj)
        {
            return Equals(obj as Person);
        }

        public bool Equals(Person person)
        {
            if (person == null) return false;
            return Name == person.Name;
        }

        public override int GetHashCode()
        {
            return Name?.GetHashCode() ?? 0;
        }
    }
}
