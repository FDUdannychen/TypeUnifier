using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace TypeUnifier
{
    public interface IException
    {
        string Source { get; set; }
        string HelpLink { get; set; }
        string StackTrace { get; }
        MethodBase TargetSite { get; }
        Exception InnerException { get; }
        string Message { get; }
        int HResult { get; }
        IDictionary Data { get; }
        Exception GetBaseException();
        void GetObjectData(SerializationInfo info, StreamingContext context);
        string ToString();
    }

    public class Exception<T> : Exception where T : IException
    {
        public T Abstraction { get; }

        internal Exception(T abstraction)
        {
            this.Abstraction = abstraction;
        }

        public override string Message => this.Abstraction.Message;
        public override IDictionary Data => this.Abstraction.Data;
        public override string HelpLink { get => this.Abstraction.HelpLink; set => this.Abstraction.HelpLink = value; }
        public override string Source { get => this.Abstraction.Source; set => this.Abstraction.Source = value; }
        public override string StackTrace => this.Abstraction.StackTrace;
        public override Exception GetBaseException() => this.Abstraction.GetBaseException();
        public override void GetObjectData(SerializationInfo info, StreamingContext context) => this.Abstraction.GetObjectData(info, context);
        public override string ToString() => this.Abstraction.ToString();        
    }
}
