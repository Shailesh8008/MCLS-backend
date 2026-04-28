namespace MCLS.Generics
{
    public class ControllerResponse<T>
    {
        public T? Data { get; set; }
        public string? Message { get; set; }
        public bool Ok { get; set; }

        public static ControllerResponse<T> Success(string message, T? data)
        {
            return new ControllerResponse<T> { Message = message, Data = data, Ok = true };
        }
        public static ControllerResponse<T> Failure(string message, T? data)
        {
            return new ControllerResponse<T> { Message = message, Data = data, Ok = false };
        }
    }
}
