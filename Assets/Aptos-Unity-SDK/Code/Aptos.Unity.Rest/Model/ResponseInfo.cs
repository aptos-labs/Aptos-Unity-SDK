namespace Aptos.Unity.Rest.Model
{
    public class ResponseInfo
    {
        public Status status;
        public string message;
        public enum Status
        {
            Success,
            Pending,
            NotFound,
            Failed,
            Warning
        }
    }
}

