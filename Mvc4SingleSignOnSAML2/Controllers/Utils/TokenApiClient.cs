﻿using ComponentSpace.SAML2;
using Mvc4SingleSignOnSAML2.Controllers.Utils;
using Mvc4SingleSignOnSAML2.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web;

namespace Mvc4SingleSignOnSAML2.Controllers.Utils
{
    public class TokenApiClient
    {
        public string GetAccessToken()
        {
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

            HttpClient httpClient = new HttpClient();
            
            // Set consumer key and secret in basic authentication header
            var buffer = Encoding.UTF8.GetBytes(Configuration.ConsumerKey + ":" + Configuration.ConsumerSecret);
            var authHeader = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(buffer));
            httpClient.DefaultRequestHeaders.Authorization = authHeader;
            
            // Set SAML assertion in message body
            string samlAssertion = SAMLAssertion.Value;
            if(String.IsNullOrEmpty(samlAssertion)) 
            {
                return null;
            }

            // There seems to be an issue in Key Manager for SAML2 grant type
            string encodedSamlAssertion = HttpUtility.UrlEncode(Convert.ToBase64String(Encoding.UTF8.GetBytes(samlAssertion)));
            string content = "grant_type=urn:ietf:params:oauth:grant-type:saml2-bearer&assertion=" + encodedSamlAssertion
                + "&scope=PRODUCTION";
            
            //string content = "grant_type=password&username=admin&password=admin";
            StringContent httpContent = new StringContent(content, Encoding.UTF8, "application/x-www-form-urlencoded");
            
            var response = httpClient.PostAsync(Configuration.TokenApiURL, httpContent);
            string responseBody = response.Result.Content.ReadAsStringAsync().Result;
            if(!String.IsNullOrEmpty(responseBody)) {
                TokenGenerationResponse responseObj = JsonConvert.DeserializeObject<TokenGenerationResponse>(responseBody);
                return responseObj.access_token;
            }
            return null;
        }
    }
}