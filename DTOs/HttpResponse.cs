using System;
using System.Collections.Generic;
using System.Net;

namespace BaseRestAPI.DTOs
{
    public partial class HttpResponse<T>
    {
        public HttpResponse(){
            ValidationDictionary = CreateValidationDictionary<T>();
        }
        public HttpStatusCode Code { get; set; }
        public T Item {get; set;}

        public bool Valid {get; set;}
        public Dictionary<string, string> ValidationDictionary {get; set;}
        public static Dictionary<string, string> CreateValidationDictionary<T>(){
            var t = typeof(T);
            var names = t.GetProperties();
            var dictionary = new Dictionary<string, string>();
            foreach(var name in names){
                dictionary.Add(name.Name, "");
            }
            return new Dictionary<string, string>();
        }
    }

    public partial class ValidationObject<T> {
        
    }
}
