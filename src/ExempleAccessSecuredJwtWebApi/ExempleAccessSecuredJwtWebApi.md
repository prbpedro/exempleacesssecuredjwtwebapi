# ASPNetCore - WebAPI JWT Token Consumer

Através deste tutorial iremos criar um projeto <b>web-api</b> com o <i>framework</i> <b>aspnetcore 2.2</b> que deverá fazer a autenticação/autorização dos usuários através de um <i>issuer</i> <b>JWT</b> externo ([DotnetCoreIdentityServerJwtIssuer](https://git.serpro/ComponentesDotNet/dotnetcoreidentityserverjwtissuer/blob/master/src/DotnetCoreIdentityServerJwtIssuer/DotnetCoreIdentityServerJwtIssuer.md)) e acessar uma <b>web-api</b> segura ([SecuredJwtWebApi](https://git.serpro/ComponentesDotNet/securedjwtwebapi/blob/master/src/SecuredJwtWebApi/SecuredJwtWebApi.md)) separada do <i>Issuer</i>.

Para tanto faremos uso do <i>framework</i> <b>ASPNetCore 2.2</b> e do editor de código fonte <b>Visual Studio Code</b>.

## Criar um projeto web-api ASPNetCore 2.2

1. Abra uma pasta que deverá conter o projeto a ser criado
1. Crie um projeto web-api ASPNetCore 2.2 através do comando abaixo no terminal <b>Windows PowerShell</b> contido no <b>VS Code</b>.

	```csharp
    dotnet new webapi
	```
	
### Editar classe Program.cs
Incluir chamada ao método <i>UseUrls</i> no método <i>CreateWebHostBuilderconforme</i>. A execução deste método determina a URL de entrada do web-api e por padrão desabilita o HTTPS no Kestrel. Em um ambiente produtivo a aplicação deverá ser acessada via protocolo HTTPS.

```csharp
public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseUrls("http://localhost:8000");
```

### Editar classe Startup.cs

#### Alterar método de configuração da aplicação

Iremos retirar as configurações relacionadas a segurança da aplicação (HTTP, HSTS), em ambientes produtivos os servidores web deverão assegurar os requisitos de segurança necessários ao acesso a esta <b>web-api</b>.

```csharp
public void Configure(IApplicationBuilder app, IHostingEnvironment env)
{
    app.UseMvc();
}
```

### Criar <i>controller</i> <b>AspNetCore MVC</b> para disponibilização de método de acesso a <b>web-api</b> segura

Iremos criar um método que deverá fazer a autenticação/autorização dos usuários através de um <i>issuer</i> <b>JWT</b> externo ([DotnetCoreIdentityServerJwtIssuer](https://git.serpro/ComponentesDotNet/dotnetcoreidentityserverjwtissuer/blob/master/src/DotnetCoreIdentityServerJwtIssuer/DotnetCoreIdentityServerJwtIssuer.md)) e acessar uma <b>web-api</b> segura ([SecuredJwtWebApi](https://git.serpro/ComponentesDotNet/securedjwtwebapi/blob/master/src/SecuredJwtWebApi/SecuredJwtWebApi.md)) separada do <i>Issuer</i>.

Será necessário criar a classe ResponseDto que representa o retorno da chamada ao <i>issuer</i> <b>JWT</b> conforme código abaixo:

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


Criar a classe AccessSecuredWebApiController conforme código abaixo:

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

Agora podemos acessar o método GET da classe AccessSecuredWebApiController através do sítio <http://localhost:8000/api/AccessSecuredWebApi>.

Deveremos ter uma resposta similar ao texto/json abaixo:

```json
{
    "retornoAcessoOkHttpStatusCode": 200,
    "retornoAcessoOk": "{\"mensagem\":\"Método que somente token com usuário com role 'administrador' pode acessar. USUARIO: name: 029ad026-fd6e-4207-909a-f78c60f7bef7, authenticated: True, claims:((Type: http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name, value: 029ad026-fd6e-4207-909a-f78c60f7bef7), (Type: http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name, value: admin@serpro.gov.br), (Type: iss, value: http://localhost:6000/), (Type: jti, value: 6c13f8cd823d45e7af64ba0f330d8cad), (Type: http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier, value: 029ad026-fd6e-4207-909a-f78c60f7bef7), (Type: http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress, value: admin@serpro.gov.br), (Type: exp, value: 1555562842), (Type: nbf, value: 1555526242), (Type: auth_time, value: 1555526242), (Type: aud, value: audience1), (Type: http://schemas.microsoft.com/ws/2008/06/identity/claims/role, value: administrador), (Type: iat, value: 1555526242), )\"}",
    "retornoAcessoNokHttpStatusCode": 403,
    "retornoAcessoNok": ""
}
```
