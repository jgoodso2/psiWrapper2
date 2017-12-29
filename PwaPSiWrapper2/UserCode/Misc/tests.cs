using System;
//using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Net;
using System.IO;
using System.Text;
using System.Collections.Specialized;

namespace PwaPSIWrapper
{
    //[TestClass]
    //public class UnitTest1
    //{
    //    [Ignore]
    //    public void TestMethod1()
    //    {
    //        var url1 = "http://nadcwdappmsp01/pwa5";
    //        var url2 = "http://nadcwdappmsp01/PWA5/_layouts/PwaPSIWrapper/PwaGateway.aspx";
    //        WebRequest request;
    //        request = WebRequest.Create(url1);
    //        request.Method = "POST";
    //        //request.
    //        request.UseDefaultCredentials = true;

    //        WebResponse response;
    //        response = request.GetResponse();
    //        Console.WriteLine(response);

    //    }

    //    [TestMethod]
    //    public void TestUpload()
    //    {
    //        var url ="http://2013web01:8088/ProjectServer1/_layouts/15/PwaPSIWrapper/PwaAdapter.aspx";
    //        WebClient client = new WebClient();
    //        client.UseDefaultCredentials = true;
    //        NameValueCollection collection = new NameValueCollection()
    //        {
    //            {"method","PwaAddResourcePlanCommand" },
    //            {"ProjUID","['c33f40fc-7e12-49ac-a878-cd63cd0cbc9d']" }
    //        };
    //        byte[] test = client.UploadValues(url, collection);
    //        string result = System.Text.Encoding.UTF8.GetString(test);
    //    }
    //    [TestMethod]
    //    public void TestPostDataSuccessful()
    //    {
    //        // Create a request using a URL that can receive a post. 
    //        WebRequest request = WebRequest.Create("http://2013web01:8088/ProjectServer1/_layouts/15/PwaPSIWrapper/PwaAdapter.aspx");
    //        // Set the Method property of the request to POST.
    //        request.Method = "POST";
    //        request.UseDefaultCredentials = true;
    //        // Create POST data and convert it to a byte array.
    //        string postData = "{'ProjUID':['c33f40fc-7e12-49ac-a878-cd63cd0cbc9d']}";

    //        byte[] byteArray = Encoding.UTF8.GetBytes(postData);
    //        // Set the ContentType property of the WebRequest.
    //        request.ContentType = "application/x-www-form-urlencoded";

    //        // Set the ContentLength property of the WebRequest.
    //        request.ContentLength = byteArray.Length;
    //        // Get the request stream.
    //        Stream dataStream = request.GetRequestStream();
    //        // Write the data to the request stream.
    //        dataStream.Write(byteArray, 0, byteArray.Length);
    //        // Close the Stream object.
    //        dataStream.Close();
    //        // Get the response.
    //        WebResponse response = request.GetResponse();
    //        // Display the status.
    //        Console.WriteLine(((HttpWebResponse)response).StatusDescription);
    //        // Get the stream containing content returned by the server.
    //        dataStream = response.GetResponseStream();
    //        // Open the stream using a StreamReader for easy access.
    //        StreamReader reader = new StreamReader(dataStream);
    //        // Read the content.
    //        string responseFromServer = reader.ReadToEnd();
    //        // Display the content.
    //        Console.WriteLine(responseFromServer);
    //        // Clean up the streams.
    //        reader.Close();
    //        dataStream.Close();
    //        response.Close();
    //    }




    //}
}

