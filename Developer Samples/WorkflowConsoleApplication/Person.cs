using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace WorkflowConsoleApplication
{
    [DataContract]
    [Serializable()]
    public class Person
    {
        private string _firstName;
        private string _salutation;

        [DataMember]
        public string FirstName
        {
            get { return _firstName; }
            set { _firstName = value; }
        }

        [DataMember]
        public string Salutation
        {
            get { return _salutation; }
            set { _salutation = value; }
        }
    }
}
