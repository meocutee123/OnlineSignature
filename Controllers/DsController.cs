using DocuSign.eSign.Client.Auth;
using Microsoft.AspNetCore.Mvc;
using OnlineSignature.Models;
using OnlineSignature.Services;
using System.Threading.Tasks;
using OnlineSignature.Ultilities;
using System;
using DocuSign.eSign.Model;
using System.Collections.Generic;
using System.IO;

namespace OnlineSignature.Controllers
{
    public class DsController : Controller
    {
        private const long THIRTY_MINUTES_IN_SECONDS = 1800;
        private readonly IExampleService _exampleService;

        public DsController(IExampleService exampleService)
        {
            _exampleService = exampleService;
        }

        #region Login
        [HttpGet]
        public async Task<IActionResult> Login()
        {
            var oAuthInfo = GetOAuthInfo();

            if (oAuthInfo is null || HasTokenExpired(oAuthInfo, out long expireIn))
            {
                var authorizationUri = _exampleService.GetAuthorizationUri();
                return View(authorizationUri);
            }

            if (expireIn < THIRTY_MINUTES_IN_SECONDS) await RequestAccessToken(oAuthInfo.AccessToken);

            return RedirectToAction(nameof(Index));
        }
        #endregion

        #region DS Callback
        [HttpGet]
        public IActionResult Callback(string code)
        {
            var (oAuthToken, defaultAccount) = _exampleService.RequestAccessToken(code);

            HttpContext.Session.SetObject("oAuthToken", oAuthToken);

            HttpContext.Session.SetObject("account", defaultAccount);

            return Redirect(nameof(Index));
        }
        #endregion

        #region Index
        public IActionResult Index()
        {
            var oAuthToken = GetOAuthInfo();
            if (oAuthToken is null) return RedirectToAction(nameof(Login));

            ViewBag.Token = oAuthToken.AccessToken;
            ViewBag.RefreshToken = oAuthToken.RefreshToken;

            return View();
        }
        [HttpGet]
        #endregion

        #region Envelope
        public IActionResult Envelope()
        {
            var oAuthInfo = GetOAuthInfo();
            if (oAuthInfo is null) return RedirectToAction(nameof(Login));

            ViewBag.RefreshToken = oAuthInfo.RefreshToken;

            var envelopeCreationRequest = new EnvelopeCreationRequest
            {
                SignerClientId = oAuthInfo.AccountId
            };
            return View(envelopeCreationRequest);
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<ActionResult<EnvelopCreationResponse>> Envelope(EnvelopeCreationRequest envelopeCreationRequest)
        {
            if (ModelState.IsValid)
            {
                var documents = new List<Document>();
                var documentID = 1;
                foreach (var pdf in envelopeCreationRequest.DocPDFs)
                {
                    using var ms = new MemoryStream();
                    pdf.CopyTo(ms);
                    var buffer = Convert.ToBase64String(ms.ToArray());
                    var document = new Document()
                    {
                        Name = pdf.Name,
                        FileExtension = "pdf",
                        DocumentBase64 = buffer,
                        DocumentId = $"{documentID}"
                    };
                    documents.Add(document);
                    documentID++;
                }

                var signHere = new SignHere
                {
                    AnchorString = "/sn1/",
                    AnchorUnits = "pixels",
                    AnchorXOffset = "100",
                    AnchorYOffset = "200"
                };

                var signer = new Signer
                {
                    Email = envelopeCreationRequest.SignerEmail,
                    Name = envelopeCreationRequest.SignerName,
                    ClientUserId = envelopeCreationRequest.SignerClientId,
                    RecipientId = "1",
                    Tabs = new Tabs { SignHereTabs = new List<SignHere> { signHere } },
                };


                var envelopeDefinition = new EnvelopeDefinition()
                {
                    EmailSubject = "Please sign this document",
                    Documents = documents,
                    Recipients = new Recipients { Signers = new List<Signer>() { signer } },
                    Status = "sent"
                };

                var oAuthInfo = GetOAuthInfo();
                var envelopeCreationInfo = new EnvelopeCreationInfo
                {
                    EnvelopeDefinition = envelopeDefinition,
                    AccessToken = oAuthInfo.AccessToken,
                    AccountId = oAuthInfo.AccountId,
                    BasePath = oAuthInfo.BasePath   
                };

                var envelopeCreationResponse = await _exampleService.SendEnvelopeForEmbeddedSigning(envelopeCreationInfo);
                return RedirectToAction(nameof(Index));
            }
            return View(envelopeCreationRequest);
        }
        #endregion

        [HttpPost]
        public async Task<ActionResult> RefreshToken(string refreshToken)
        {
            await RequestAccessToken(refreshToken);
            return RedirectToAction(nameof(Index));
        }

        private OAuthInfo GetOAuthInfo()
        {
            var oAuthToken = HttpContext.Session.GetObject<OAuth.OAuthToken>("oAuthToken");
            var account = HttpContext.Session.GetObject<OAuth.UserInfo.Account>("account");

            if (oAuthToken is null || account is null) return null;

            return new OAuthInfo { AccessToken = oAuthToken.access_token, RefreshToken = oAuthToken.refresh_token, AccountId = account.AccountId, BasePath = account.BaseUri };
        }
        private async Task RequestAccessToken(string refreshToken)
        {
            var (oAutToken, account) = await _exampleService.RefreshToken(refreshToken);
            HttpContext.Session.SetObject("oAuthToken", oAutToken);
            HttpContext.Session.SetObject("account", account);
        }
        private bool HasTokenExpired(OAuthInfo oAuthInfo, out long expireIn)
        {
            var accessToken = oAuthInfo.AccessToken;
            var origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            var expireDate = origin.AddSeconds((double)oAuthInfo.ExpiresAt);

            expireIn = 2000;
            return false;
        }
    }
}
