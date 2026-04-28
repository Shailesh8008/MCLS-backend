namespace MCLS.Generics
{
    public class ServiceResponse<T>
    {
        public string? Message { get; set; }
        public T? Data { get; set; }
        public bool Ok { get; set; }
        public int Status { get; set; }

        public static ServiceResponse<T> Success(string message, T? data, int status = 200)
        {
            return new ServiceResponse<T> { Message = message, Data = data, Ok = true, Status = status };
        }
        public static ServiceResponse<T> Failure(string message, T? data, int status = 500)
        {
            return new ServiceResponse<T> { Message = message, Data = data, Ok = false, Status = status };
        }
    }
}
