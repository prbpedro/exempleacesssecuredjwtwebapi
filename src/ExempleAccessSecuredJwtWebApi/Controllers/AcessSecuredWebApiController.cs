using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using ExempleAccessSecuredJwtWebApi.Dto;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace ExempleAccessSecuredJwtWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccessSecuredWebApiController : ControllerBase
    {
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            ResponseDto responseDto;
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                HttpResponseMessage respToken = client.PostAsync(
                    "http://localhost:6000/api/Login", 
                    new StringContent
                    (
                        JsonConvert.SerializeObject(new
                        {
                            Audience = "audience1",
                            UserEmail = "admin@serpro.gov.br",
                            UserPassword = "Sw0rdfi$h"
                        }
                    ), Encoding.UTF8, "application/json")
                ).Result;

                responseDto = (ResponseDto)respToken.Content.ReadAsAsync(typeof(ResponseDto)).Result;
            }

            string retornoAcessoOk;
            HttpStatusCode retornoAcessoOkHttpStatusCode;
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", responseDto.AccessToken);
                HttpResponseMessage respToken = client.GetAsync("http://localhost:7000/api/secured/administrador").Result;
                retornoAcessoOkHttpStatusCode = respToken.StatusCode;
                retornoAcessoOk = respToken.Content.ReadAsStringAsync().Result;
            }

            string retornoAcessoNok;
            HttpStatusCode retornoAcessoNokHttpStatusCode;
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", responseDto.AccessToken);
                HttpResponseMessage respToken = client.GetAsync("http://localhost:7000/api/secured/usuario").Result;
                retornoAcessoNokHttpStatusCode = respToken.StatusCode;
                retornoAcessoNok = respToken.Content.ReadAsStringAsync().Result;
            }

            return Ok(new {retornoAcessoOkHttpStatusCode = retornoAcessoOkHttpStatusCode, retornoAcessoOk = retornoAcessoOk, retornoAcessoNokHttpStatusCode = retornoAcessoNokHttpStatusCode, retornoAcessoNok = retornoAcessoNok});
        }
    }
}
