# ASPNetCore - WEB-API JWT Token Consumer
[![Build Status](https://travis-ci.org/prbpedro/exempleacesssecuredjwtwebapi.svg?branch=master)](https://travis-ci.org/prbpedro/exempleacesssecuredjwtwebapi)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

Through this tutorial you'll be able to create an ASPNetCore 2.2 WEB-API that will authenticate and authorize users through an external JWT issuer ([DotnetCoreIdentityServerJwtIssuer](https://github.com/prbpedro/dotnetcoreidentityserverjwtissuer)) and acess a secured WEB-API ([SecuredJwtWebApi](https://github.com/prbpedro/securedjwtwebapi)) apart from the mentioned issuer.

## Creating an ASPNetCore 2.2 WEB-API project

1. Open the folder where you want the project to be in the VS Code
1. Create an ASPNetCore 2.2 WEB-API project throw running the below command in the Windows PowerShell Terminal in the VS Code

	```csharp
    dotnet new webapi
	```
	
### Editing the Program.cs class

You must include the call to the method UseUrls in the CreateWebHostBuilder delegate method. The execution of this method disables the HTTPS protocol on Kestrel and defines the entry URL of the WEB-API project created. 

In a productive environment the application must be acess throw the HTTPS protocol.

```csharp
public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseUrls("http://localhost:8000");
```

### Editing the Startup.cs class

#### Modifying the application configuration method

Remove the application security configurations. In productive environments, the WEB servers must ensure the security requirements needed to access this WEB-API.

```csharp
public void Configure(IApplicationBuilder app, IHostingEnvironment env)
{
    app.UseMvc();
}
```

### Creating an AspNetCore MVC Controller to provide the access methods to the secured WEB-API

Create a method that will authenticate and authorize users through an external JWT issuer ([DotnetCoreIdentityServerJwtIssuer](https://github.com/prbpedro/dotnetcoreidentityserverjwtissuer)) and access a secured WEB-API ([SecuredJwtWebApi](https://github.com/prbpedro/securedjwtwebapi)) apart from the mentioned issuer.

Create the class ResponseDto.cs that represents the call return of the JWT issuer as the below code:

```csharp
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
```
Create the AccessSecuredWebApiController.cs class as the below code:

```csharp
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
```

Now you can call the GET method of the AccessSecuredWebApiController.cs class through the site <http://localhost:8000/api/AccessSecuredWebApi>.

The GET method should return A JSON text response similar to the below:

```json
{
    "retornoAcessoOkHttpStatusCode": 200,
    "retornoAcessoOk": "{\"mensagem\":\"Método que somente token com usuário com role 'administrador' pode acessar. USUARIO: name: 029ad026-fd6e-4207-909a-f78c60f7bef7, authenticated: True, claims:((Type: http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name, value: 029ad026-fd6e-4207-909a-f78c60f7bef7), (Type: http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name, value: admin@serpro.gov.br), (Type: iss, value: http://localhost:6000/), (Type: jti, value: 6c13f8cd823d45e7af64ba0f330d8cad), (Type: http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier, value: 029ad026-fd6e-4207-909a-f78c60f7bef7), (Type: http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress, value: admin@serpro.gov.br), (Type: exp, value: 1555562842), (Type: nbf, value: 1555526242), (Type: auth_time, value: 1555526242), (Type: aud, value: audience1), (Type: http://schemas.microsoft.com/ws/2008/06/identity/claims/role, value: administrador), (Type: iat, value: 1555526242), )\"}",
    "retornoAcessoNokHttpStatusCode": 403,
    "retornoAcessoNok": ""
}
```
