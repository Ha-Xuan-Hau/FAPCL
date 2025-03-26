namespace FAPCL.DTO.ExamSchedule
{
    public class ServiceResult<T>
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public T Data { get; set; }

        public static ServiceResult<T> Success(T data) => new ServiceResult<T> { IsSuccess = true, Data = data };
        public static ServiceResult<T> Failure(string message) => new ServiceResult<T> { IsSuccess = false, Message = message };
    }

}
