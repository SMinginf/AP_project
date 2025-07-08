using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizClient.Utils
{
    public class ServiceResult<T>
    {
        public T? Data { get; set; }
        public bool Success => ErrorMessage == null;
        public string? ErrorMessage { get; set; }
    }
}
