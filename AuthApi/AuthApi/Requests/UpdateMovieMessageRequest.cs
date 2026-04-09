namespace AuthApi.Requests
{
    public class UpdateMovieMessageRequest
    {
        public int id_Message { get; set; }
        public string? text { get; set; }
        public IFormFile? newFile { get; set; }
        public bool removeCurrentImage { get; set; } = false;
    }
}
