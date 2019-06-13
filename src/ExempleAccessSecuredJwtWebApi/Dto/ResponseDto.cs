namespace ExempleAccessSecuredJwtWebApi.Dto
{
    public class ResponseDto
    {
        public bool Authorized {get; set;}

        public string Created {get; set;}

        public string Expires {get; set;}

        public string AccessToken {get; set;}
    }
}