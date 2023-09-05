using AuthenticationApi.Db;
using AuthenticationApi.Dtos;
using AuthenticationApi.Entities;
using AuthenticationApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace AuthenticationApi.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{
    private readonly IAuthenticationService _authenticationService;
    private AppDbContext appDbContext;
    IConfiguration configuration;

    public UserController(IAuthenticationService authenticationService,AppDbContext appDbContext, IConfiguration configuration)
    {
        _authenticationService = authenticationService;
        this.appDbContext = appDbContext;
        this.configuration = configuration;

    }

    //[AllowAnonymous]
    //[HttpPost("login")]
    //[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
    //[ProducesResponseType(StatusCodes.Status500InternalServerError)]
    //[ProducesResponseType(StatusCodes.Status400BadRequest)]
    //public async Task<IActionResult> Login([FromBody] LoginRequest request)
    //{
    //    var response = await _authenticationService.Login(request);

    //    return Ok(response);
    //}
    [AllowAnonymous]
    [HttpPost("login")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Login([FromBody] LoginRequest user)
    {
        IActionResult response = Unauthorized();

        if (user != null)
        {
            if (user.Username.Equals("test@gmail.com") && user.Password.Equals("pwd"))
            {
                var issuer = configuration["JWT:Issuer"];
                var audience = configuration["JWT:Audience"];
                var key = Encoding.UTF8.GetBytes(configuration["JWT:Key"]);
                var signingCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha512Signature);
                var subject = new ClaimsIdentity(new[]
                { new Claim(JwtRegisteredClaimNames.Sub,user.Username),
                      new Claim(JwtRegisteredClaimNames.Email,user.Username)});

                var expires = DateTime.UtcNow.AddDays(10);

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = subject,
                    Expires = expires,
                    Issuer = issuer,
                    Audience = audience,
                    SigningCredentials = signingCredentials
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var token = tokenHandler.CreateToken(tokenDescriptor);
                var jwtToken = tokenHandler.WriteToken(token);

                return Ok(jwtToken);

            }
            else { return Ok("fail"); };

        }
        return response;
    }

    [AllowAnonymous]
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var response = await _authenticationService.Register(request);

        return Ok(response);
    }

    [AllowAnonymous]
    [HttpPost("getclaims")]
    //[Authorize]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ResponseMessage GetClaims(dynamic sendinfo)
    {
        var jsonString = JsonConvert.DeserializeObject<dynamic>(sendinfo.ToString());
        string claimNumber=jsonString.ClaimNumberValue.ToString();

       

        sunClaimHdrCodesData objsunClaimMedCodesData = new sunClaimHdrCodesData
        {
            ttchControlNumber = "E119600069",
            ttintCdCount = 1,
            ttchCdType = "DiagCode",
            ttchCdCode = "V700",
            ttchCdDesc = "Driver of bus injured",
            ttchCdQualifier = "",
            ttchCdPOA = ""
        };

        SunClaimDetailMap objSunClaimDetailMap1 = new SunClaimDetailMap
        {
            ttmcsMCHLineNum = "1",
            ttmcsMCHDiagCode = "1",
            ttmcsMCHProcCode = "T1015",
            ttmcsMCSCharge = "131.48",
            ttmcsMCSAllowed = "0",
            ttmcsMCSCoInsurance = "0",
            ttmcsMCSCoPay = "0",
            ttmcsMCSStatus = "F1"

        };

        SunClaimDetailMap objSunClaimDetailMap2 = new SunClaimDetailMap
        {
            ttmcsMCHLineNum = "2",
            ttmcsMCHDiagCode = "1",
            ttmcsMCHProcCode = "99395",
            ttmcsMCSCharge = "0",
            ttmcsMCSAllowed = "0",
            ttmcsMCSCoInsurance = "0",
            ttmcsMCSCoPay = "0",
            ttmcsMCSStatus = "F2"

        };

        List<SunClaimDetailMap> lstSunClaimDetailMap = new List<SunClaimDetailMap>()
        {
            objSunClaimDetailMap1,
            objSunClaimDetailMap2
        };




        SunWebClaimHdr objttWebClaimHdr =new SunWebClaimHdr
        {
            ttchMCS = "MCS-Response from API",
            ttdecadjustmentamount=0.0M,
            ttdadjustmentdate="2011-04-07",
            ttchbatch="528",
            ttchclaimNote="",
            ttchclaimType="",
            ttchclaimSubType="",
            ttchclaimNumber="16843145",
            ttdcreateDate="2011-04-06",
            ttdchcreateUser="postedi.r",
            ttdchStatus="F1-Paid"           

        };
        if (claimNumber == objttWebClaimHdr.ttchclaimNumber)
        {
            ResponseMessage msg = new ResponseMessage
            {
                ttWebClaimHdr = objttWebClaimHdr,
                ttwebClaimHdrCodes = objsunClaimMedCodesData,
                ttClaimDtlMap = lstSunClaimDetailMap
            };
            return msg;

        }
        else
            return new ResponseMessage();
    }
}

