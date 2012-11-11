using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Fiddler;
using System.Threading;
using Sniffer.Entity;
using Sniffer.Code;

namespace Sniffer
{
    /// <summary>
    /// Options to run the sniffer
    /// </summary>
    public enum Config
    {
        Run, Debug, ShowResponses, CaptureAll, CaptureOnlyWithRequestParameters
    }

    /// <summary>
    /// This class contains all the required methods for doing data sniffing
    /// </summary>
    class Sniffer
    {
        /// <summary>
        /// The port at which the sniffer listens to as a proxy
        /// </summary>
        private static Int32 _PORT = 8877;

        private static Sniffer _objSniffer;

        private static Config _enConfiguration = Config.Run;



        /// <summary>
        /// Private constructor
        /// </summary>
        private Sniffer()
        {

        }

       
        /// <summary>
        /// Returns the Sniffer instance
        /// </summary>
        /// <returns></returns>
        public static Sniffer GetSniffer()
        {
            if (_objSniffer == null)
                _objSniffer = new Sniffer();

            return _objSniffer;
        }

        /// <summary>
        /// Sets the sniffer configuration
        /// </summary>
        public static Config Configuration
        {
            get { return Sniffer._enConfiguration; }
            set { Sniffer._enConfiguration = value; }
        }

        /// <summary>
        /// This method starts the sniffing process
        /// </summary>
        public void Start()
        {
            try
            {
                //Add the listeners
                FiddlerApplication.BeforeRequest += new SessionStateHandler(FiddlerApplication_BeforeRequest);
                FiddlerApplication.BeforeResponse += new SessionStateHandler(FiddlerApplication_BeforeResponse);

                // For the purposes of this demo, we'll forbid connections to HTTPS 
                // sites that use invalid certificates
                //CONFIG.IgnoreServerCertErrors = false;

                //Creates the certificate if required
                CreateCertificateIfRequired();

                //Start the fiddler for listening HTTP/HTTPS requests
                FiddlerApplication.Startup(_PORT, true, true);
            }
            catch (Exception ex)
            {
                Utility.DisplayException("Start", ex);
            }
        }


        /// <summary>
        /// Gets triggered before the request has been made
        /// </summary>
        /// <param name="objSession"></param>
        private void FiddlerApplication_BeforeRequest(Session objSession)
        {
            try
            {
                //Declarations
                Utility objUtility = new Utility();
                DBUtility objDBUtility = new DBUtility();

               
                //Declarations
                String strContentType = String.Empty;
                                
                //Uncomment this if tampering of response is required
                //objSession.bBufferResponse = true;
                
                //Get the content type
                strContentType = objSession.oRequest.headers["Accept"];

                //If its an HTML request or else the configuration has been set to capture all the requests
                if (strContentType.Contains("text/html") || _enConfiguration == Config.CaptureAll)
                {
                    //Get the request headers
                    HTTPRequestHeaders objRequestHeaders = objSession.oRequest.headers;

                    //Construct the network data
                    NetworkData objNetworkData = new NetworkData
                    {
                        ClientIP = objSession.clientIP,
                        HostName = objSession.hostname,
                        URLFullPath = objSession.fullUrl,
                        IsHTTPS = objSession.isHTTPS,
                        RequestedAt = objSession.Timers.ClientBeginRequest.ToString(),
                        RequestType = objRequestHeaders.HTTPMethod
                    };
                                
                                                           
                    //Get the request body
                    String strRequestBody = objSession.GetRequestBodyAsString();
                    

                    //If its a POST request
                    if (objNetworkData.RequestType == "POST")
                        //Get the request parameters
                        objNetworkData.RequestParameters = objUtility.GetRequestParameters(strRequestBody);
                    else if (objNetworkData.RequestType == "GET")
                    {
                        String [] arrQueryString = objNetworkData.URLFullPath.Split(new Char[] { '?' }); 

                        if(arrQueryString.Length > 1)
                            objNetworkData.RequestParameters = objUtility.GetRequestParameters(arrQueryString[1]);
                    }

    
                    //Update the capture to Mongo DB
                    if (_enConfiguration != Config.CaptureOnlyWithRequestParameters || objNetworkData.RequestParameters.Count > 0)
                        objDBUtility.AddData("NetworkData", "NetworkData", objNetworkData);                                       
                }
            }
            catch (Exception ex)
            {
                Utility.DisplayException("FiddlerApplication_BeforeRequest", ex);                                
            }
        }

        /// <summary>
        /// Gets triggered before the response gets rendered
        /// </summary>
        /// <param name="objSession"></param>
        private void FiddlerApplication_BeforeResponse(Session objSession)
        {
            String strContentType;

            //Get the content type
            strContentType = objSession.oRequest.headers["Accept"];


            String strRequestBody = objSession.GetResponseBodyAsString();
            //objSession.utilSetResponseBody("<html><body><h1>Hii</h1></body></html>");

            //If the user has opted to display responses
            if(_enConfiguration == Config.ShowResponses)
                Console.WriteLine("{0}:HTTP {1} for {2}", objSession.id, objSession.responseCode, objSession.fullUrl);
        }

        /// <summary>
        /// Shuts down the fiddler application
        /// </summary>
        internal void ShutDown()
        {
            try
            {
                FiddlerApplication.Shutdown();
            }
            catch (Exception ex)
            {                
                Utility.DisplayException("ShutDown", ex);
            }
        }


        /// <summary>
        /// Creates the certificates if required
        /// </summary>
        private void CreateCertificateIfRequired()
        {
            try
            {
                //FiddlerApplication.CreateProxyEndpoint(_PORT, true, "localhost");

                //CONFIG.IgnoreServerCertErrors = false;

                if (!Fiddler.CertMaker.rootCertExists())
                {
                    if (!Fiddler.CertMaker.createRootCert())
                    {
                        throw new Exception("Unable to create cert for FiddlerCore.");
                    }
                }

                if (!Fiddler.CertMaker.rootCertIsTrusted())
                {
                    if (!Fiddler.CertMaker.trustRootCert())
                    {
                        throw new Exception("Unable to install FiddlerCore's cert.");
                    }
                }
            }
            catch (Exception ex)
            {
                Utility.DisplayException("CreateCertificateIfRequired", ex);
            }

        }


    }
}
