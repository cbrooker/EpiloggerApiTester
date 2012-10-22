using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Windows.Forms;


namespace EpiloggerAPITest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }


        private string MakeRequestWithHeaderData(string url, string userName, string strpassword)
        {
            try
            {
                // Encode the user name with password
                var userPass = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(userName + ":" + strpassword));

                // Create our HTTP web request object
                var request = (HttpWebRequest)WebRequest.Create(url);
                textBox3.Text = request.RequestUri.AbsoluteUri;

                // Set up our request flags and submit type
                request.Method = "GET";
                request.ContentType = "application/x-www-form-urlencoded";

                // Add the authorization header with the encoded user name and password
                request.Headers.Add("Authorization", "Basic " + userPass);

                // Use an HttpWebResponse object to handle the response from Twitter
                var webResponse = (HttpWebResponse)request.GetResponse();

                // Success if we get an OK response
                var result = webResponse.StatusCode == HttpStatusCode.OK;

                var receiveStream = webResponse.GetResponseStream();
                string returnValue = null;
                if (receiveStream != null)
                {
                    var readStream = new StreamReader(receiveStream, Encoding.UTF8);
                    returnValue = readStream.ReadToEnd();
                    textBox1.Text = returnValue;

                    webResponse.Close();
                    readStream.Close();
                }

                return returnValue;

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error: " + ex.Message);
            }


            return null;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            textBox1.Text = "";
            textBox3.Text = "";

            try
            {
                const string clientId = "B65433C4E522F68117C9AB272E2797B3B87416C786847CC1EFFC6BEC63A7C402C77755056A2A452B935FA465F77932BD5CCEC807857978EA9C018FDA4C5B2CE2A1310A130B896154985D8D6D72DA56D36D73C91DF35B26A8F438FD2F13A1E28778E6EFAAFFF959C31316FD29A9E8AC2F31F5C1639678D2665831863EB47F33B7BE8C2A53BCD64E995AC15946C69CB76F75635D8679BA43BC6444CBF9FD32CC337D9EBF482EEC0177C98B82EC2D77066F0F9EE84F2CA3C0CB298FC69FF67C99476568AB47AA43246444CC74217297D3141C59BD2A346DC3396C5EAD9D6C3B3385CBE87AF6F247A873C9893AA08629D4ACB93D1D9C5C882837";
                const string clientSecret = "4B5FD11989598BE3AD5539E1BB2BF37BE3A2D5C616B247DF864889245D05E4499E14BAA1139B875A8B65955C93C2379ED3BCBDA9F467C1C7196785B2CD1403969913484CD64909F6F55429B629093A11DBCB8B9DB9A43ACA38BA5B442F52030A1C492B15EB4222F1B11B943697BAA7A4562DB1C8289ABACDAE1C18D25C61FF7D2C68D958355237F3C1415DED412B2FBE88852D7B1815CBE4F4591C21BD9C826AE0966C87CCEA5E92E99934E5ECB96DB3858717185BFAD6CEBF3C7365F6E08F62FDFE38EE5C25E75169EBD31D5815693BAC83479674B4AEB652C732106FBEAB5FDCA5D771EE430596350B65BB9F6F2E4C89A5F292E479D36AF7DA1";
                var timeStamp = (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000;
                var methodPath = new Uri(textBox2.Text); //new Uri("http://localhost:16007/Api/Categories");

                
                
                
                var signature = getHMACMD5Signature(methodPath.AbsolutePath, "", timeStamp.ToString(CultureInfo.InvariantCulture), clientId, clientSecret);
                signature = HttpUtility.UrlEncode(signature);

                if (username.Enabled)
                {
                    //Makes the request with header values.
                    MakeRequestWithHeaderData(methodPath + string.Format("?clientid={0}&timestamp={1}&signature={2}", clientId, timeStamp, signature), username.Text, password.Text);
                }
                else
                {
                    var req = WebRequest.Create(methodPath + string.Format("?clientid={0}&timestamp={1}&signature={2}", clientId, timeStamp, signature));

                    textBox3.Text = req.RequestUri.AbsoluteUri;

                    var resp = req.GetResponse();
                    var receiveStream = resp.GetResponseStream();
                    if (receiveStream != null)
                    {
                        var readStream = new StreamReader(receiveStream, Encoding.UTF8);
                        var returnValue = readStream.ReadToEnd();
                        textBox1.Text = returnValue;

                        resp.Close();
                        readStream.Close();
                    }    
                }
                

            }
            catch (Exception ex)
            {
                textBox1.Text = ex.Message;
            }

            
        }


        private static string getHMACMD5Signature(string methodPath, string additionalParameters, string timeStamp, string accessKey, string secretKey)
        {
            //build the uri to sign, exclude the domain part, start with '/'
            var uristring = new StringBuilder(); //will hold the final uri
            uristring.Append(methodPath.StartsWith("/") ? "" : "/"); //start with a slash
            uristring.Append(methodPath); //this is the address of the resource
            uristring.Append("?clientid=" + HttpUtility.UrlEncode(accessKey)); //url encoded parameters
            uristring.Append("&timestamp=" + HttpUtility.UrlEncode(timeStamp));
            uristring.Append(additionalParameters);

            //calculate hmac signature
            byte[] secretBytes = Encoding.ASCII.GetBytes(secretKey);
            var hmac = new HMACMD5(secretBytes);
            byte[] dataBytes = Encoding.ASCII.GetBytes(uristring.ToString());
            byte[] computedHash = hmac.ComputeHash(dataBytes);
            string computedHashString = Convert.ToBase64String(computedHash);

            return computedHashString;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            textBox1.Text = "";
            textBox3.Text = "";
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            const string domain = "http://localhost:16007";
            //const string domain = "http://epilogger.com";
            //const string domain = "http://epilogger.apphb.com";
            //const string domain = "http://local.epilogger.com";

            var endpoints = new List<EndPoint>();
            endpoints.Add(new EndPoint { Name = "All Categories", EndPointURL = domain + "/api/categories" });
            endpoints.Add(new EndPoint { Name = "All Events (paged)", EndPointURL = domain + "/api/events/1/3" });
            endpoints.Add(new EndPoint { Name = "Event by ID", EndPointURL = domain + "/api/events/event/133" });
            endpoints.Add(new EndPoint { Name = "Events by CategoryId (paged)", EndPointURL = domain + "/api/categories/3/Events/1/3" });
            endpoints.Add(new EndPoint { Name = "Trending Events", EndPointURL = domain + "/api/events/trending" });
            endpoints.Add(new EndPoint { Name = "Search Events", EndPointURL = domain + "/api/events/search/epilogger" });
            endpoints.Add(new EndPoint { Name = "Featured Events", EndPointURL = domain + "/api/events/featured" });
            endpoints.Add(new EndPoint { Name = "Search In Event", EndPointURL = domain + "/Api/Events/Event/1528/SearchInEvent/Big%20pimp" });
            endpoints.Add(new EndPoint { Name = "My Events", EndPointURL = domain + "/Api/Events/Me/AFA23475-0971-4795-BDDC-70F5437150FE/1/10" });
            endpoints.Add(new EndPoint { Name = "Subscribed Events", EndPointURL = domain + "/Api/Events/Me/Subscribed/AFA23475-0971-4795-BDDC-70F5437150FE/1/10" });
            endpoints.Add(new EndPoint { Name = "", EndPointURL = domain + "/api/" });

            endpoints.Add(new EndPoint { Name = "Tweets by Event (paged)", EndPointURL = domain + "/api/tweets/133/1/10" });
            endpoints.Add(new EndPoint { Name = "All Feed by Event (paged)", EndPointURL = domain + "/api/allfeed/133/1/10" });
            endpoints.Add(new EndPoint { Name = "Top 10 Tweeters By Event", EndPointURL = domain + "/api/tweets/133/top10tweeters" });
            endpoints.Add(new EndPoint { Name = "Tweets By ImageID (paged)", EndPointURL = domain + "/api/tweets/133/image/493192/1/10" });
            endpoints.Add(new EndPoint { Name = "Tweet Count By ImageID", EndPointURL = domain + "/api/tweets/133/image/493192/count" });
            endpoints.Add(new EndPoint { Name = "", EndPointURL = domain + "/api/" });
            endpoints.Add(new EndPoint { Name = "Image Count By EventID", EndPointURL = domain + "/Api/Images/133/count" });
            endpoints.Add(new EndPoint { Name = "Top Images By EventID", EndPointURL = domain + "/Api/Images/133/Top/5" });
            endpoints.Add(new EndPoint { Name = "Newest Photos By EventID", EndPointURL = domain + "/Api/Images/133/Newest/5" });
            endpoints.Add(new EndPoint { Name = "Images By Event (paged)", EndPointURL = domain + "/Api/Images/133/1/10" });
            endpoints.Add(new EndPoint { Name = "Images By EventID Order Desc Take X", EndPointURL = domain + "/Api/Images/133/5" });
            endpoints.Add(new EndPoint { Name = "Top Photos And Tweet By EventID", EndPointURL = domain + "/Api/ImagesTweets/133/5" });
            endpoints.Add(new EndPoint { Name = "", EndPointURL = domain + "/api/" });
            endpoints.Add(new EndPoint { Name = "Check In Count By Event", EndPointURL = domain + "/api/CheckIns/133/count" });
            endpoints.Add(new EndPoint { Name = "Check Ins By Event Paged", EndPointURL = domain + "/api/CheckIns/133/1/10" });
            endpoints.Add(new EndPoint { Name = "", EndPointURL = domain + "/api/" });
            endpoints.Add(new EndPoint { Name = "User By ID", EndPointURL = domain + "/Api/Users/User/AFA23475-0971-4795-BDDC-70F5437150FE" });
            endpoints.Add(new EndPoint { Name = "User By Username", EndPointURL = domain + "/api/Users/User/byname/cbrooker" });
            endpoints.Add(new EndPoint { Name = "User By Email", EndPointURL = domain + "/api/Users/User/byemail/cbrooker@gmail.com" });
            endpoints.Add(new EndPoint { Name = "User Subscribes to an Event (can't be tested here)", EndPointURL = domain });
            endpoints.Add(new EndPoint { Name = "User Unsubscribes from an Event", EndPointURL = domain + "/Api/users/user/DeleteUserEventSubscription/AFA23475-0971-4795-BDDC-70F5437150FE/187" });
            endpoints.Add(new EndPoint { Name = "", EndPointURL = domain + "/api/" });
            endpoints.Add(new EndPoint { Name = "Venue By Id", EndPointURL = domain + "/api/Venues/803" });
            endpoints.Add(new EndPoint { Name = "Venue By FourSquare Venue Id", EndPointURL = domain + "/api/Venues/foursquarevenueid/4ad4c05bf964a520a7f520e3" });
            endpoints.Add(new EndPoint { Name = "", EndPointURL = domain + "/api/" });

            endpoints.Add(new EndPoint { Name = "Validate Epilogger User Account", EndPointURL = domain + "/Api/Users/Authenticate" });

            endpoints.Add(new EndPoint { Name = "", EndPointURL = domain + "/api/" });

            endpoints.Add(new EndPoint { Name = "Remove Item from Memory Box", EndPointURL = domain + "/Api/MemoryBoxes/MemoryBox/RemoveItem/1" });
            endpoints.Add(new EndPoint { Name = "View all the Items in the Mobile Memory Box (paged)", EndPointURL = domain + "/Api/MemoryBoxes/MemoryBox/1/1/10" });
            endpoints.Add(new EndPoint { Name = "View all the Tweets in the Mobile Memory Box (paged)", EndPointURL = domain + "/Api/MemoryBoxes/MemoryBox/1/Tweets/1/10" });
            endpoints.Add(new EndPoint { Name = "View all the Photos in the Mobile Memory Box (paged)", EndPointURL = domain + "/Api/MemoryBoxes/MemoryBox/1/Photos/1/10" });
            endpoints.Add(new EndPoint { Name = "", EndPointURL = domain + "/api/" });
            endpoints.Add(new EndPoint { Name = "Get All Memory Boxes By UserId", EndPointURL = domain + "/Api/MemoryBoxes/AFA23475-0971-4795-BDDC-70F5437150FE" });
            endpoints.Add(new EndPoint { Name = "Get All Memory Boxes By UserId and EventId", EndPointURL = domain + "/Api/MemoryBoxes/AFA23475-0971-4795-BDDC-70F5437150FE/133" });
            endpoints.Add(new EndPoint { Name = "", EndPointURL = domain + "/api/" });
            endpoints.Add(new EndPoint { Name = "", EndPointURL = domain + "/api/" });
            endpoints.Add(new EndPoint { Name = "", EndPointURL = domain + "/api/" });
            endpoints.Add(new EndPoint { Name = "", EndPointURL = domain + "/api/" });
            endpoints.Add(new EndPoint { Name = "", EndPointURL = domain + "/api/" });
            endpoints.Add(new EndPoint { Name = "", EndPointURL = domain + "/api/" });
            endpoints.Add(new EndPoint { Name = "", EndPointURL = domain + "/api/" });
            endpoints.Add(new EndPoint { Name = "", EndPointURL = domain + "/api/" });
            endpoints.Add(new EndPoint { Name = "", EndPointURL = domain + "/api/" });





            listBox1.DataSource = endpoints;
            listBox1.DisplayMember = "Name";
            listBox1.ValueMember = "EndPointURL";
        }

        private class EndPoint
        {
            public string Name { get; set; }
            public string EndPointURL { get; set; }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            var si = (EndPoint)listBox1.SelectedItem;
            textBox2.Text = si.EndPointURL;

            if (si.Name == "Validate Epilogger User Account")
            {
                username.Enabled = true;
                password.Enabled = true;
            }
            else
            {
                username.Enabled = false;
                password.Enabled = false;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var testURL = "http://local.epilogger.com/Api/MemoryBoxes/MemoryBox/AddItem";


            const string clientId = "B65433C4E522F68117C9AB272E2797B3B87416C786847CC1EFFC6BEC63A7C402C77755056A2A452B935FA465F77932BD5CCEC807857978EA9C018FDA4C5B2CE2A1310A130B896154985D8D6D72DA56D36D73C91DF35B26A8F438FD2F13A1E28778E6EFAAFFF959C31316FD29A9E8AC2F31F5C1639678D2665831863EB47F33B7BE8C2A53BCD64E995AC15946C69CB76F75635D8679BA43BC6444CBF9FD32CC337D9EBF482EEC0177C98B82EC2D77066F0F9EE84F2CA3C0CB298FC69FF67C99476568AB47AA43246444CC74217297D3141C59BD2A346DC3396C5EAD9D6C3B3385CBE87AF6F247A873C9893AA08629D4ACB93D1D9C5C882837";
            const string clientSecret = "4B5FD11989598BE3AD5539E1BB2BF37BE3A2D5C616B247DF864889245D05E4499E14BAA1139B875A8B65955C93C2379ED3BCBDA9F467C1C7196785B2CD1403969913484CD64909F6F55429B629093A11DBCB8B9DB9A43ACA38BA5B442F52030A1C492B15EB4222F1B11B943697BAA7A4562DB1C8289ABACDAE1C18D25C61FF7D2C68D958355237F3C1415DED412B2FBE88852D7B1815CBE4F4591C21BD9C826AE0966C87CCEA5E92E99934E5ECB96DB3858717185BFAD6CEBF3C7365F6E08F62FDFE38EE5C25E75169EBD31D5815693BAC83479674B4AEB652C732106FBEAB5FDCA5D771EE430596350B65BB9F6F2E4C89A5F292E479D36AF7DA1";
            var timeStamp = (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000;
            var methodPath = new Uri(testURL); //new Uri("http://localhost:16007/Api/Categories");

            var signature = getHMACMD5Signature(methodPath.LocalPath, "", timeStamp.ToString(CultureInfo.InvariantCulture), clientId, clientSecret);
            signature = HttpUtility.UrlEncode(signature);

            var request = WebRequest.Create(methodPath + string.Format("?clientid={0}&timestamp={1}&signature={2}", clientId, timeStamp, signature));

            
            request.Method = "POST";
            // Create POST data and convert it to a byte array.
            var postData = "UserId=3fe7b8f8-90e5-48e7-b747-94a767631253&ItemId=24633511&AddedDateTime=&ItemType='Tweet'&MemboxId=24633511&ID=&EventId=1667";
            var byteArray = Encoding.UTF8.GetBytes(postData);
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = byteArray.Length;
            var dataStream = request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();

            textBox3.Text = request.RequestUri.AbsoluteUri;

            var resp = request.GetResponse();
            var receiveStream = resp.GetResponseStream();
            if (receiveStream != null)
            {
                var readStream = new StreamReader(receiveStream, Encoding.UTF8);
                var returnValue = readStream.ReadToEnd();
                textBox1.Text = returnValue;

                resp.Close();
                readStream.Close();
            }    



            //// Get the response.
            //WebResponse response = request.GetResponse();
            //// Display the status.
            //Console.WriteLine(((HttpWebResponse)response).StatusDescription);
            //// Get the stream containing content returned by the server.
            //dataStream = response.GetResponseStream();
            //// Open the stream using a StreamReader for easy access.
            //StreamReader reader = new StreamReader(dataStream);
            //// Read the content.
            //string responseFromServer = reader.ReadToEnd();
            //// Display the content.
            //Console.WriteLine(responseFromServer);
            //// Clean up the streams.
            //reader.Close();
            //dataStream.Close();
            //response.Close();


            


        }

        private void button4_Click(object sender, EventArgs e)
        {
            var testURL = "http://local.epilogger.com/Api/Users/Authenticate/DisconnectFacebookAccount";


            const string clientId = "B65433C4E522F68117C9AB272E2797B3B87416C786847CC1EFFC6BEC63A7C402C77755056A2A452B935FA465F77932BD5CCEC807857978EA9C018FDA4C5B2CE2A1310A130B896154985D8D6D72DA56D36D73C91DF35B26A8F438FD2F13A1E28778E6EFAAFFF959C31316FD29A9E8AC2F31F5C1639678D2665831863EB47F33B7BE8C2A53BCD64E995AC15946C69CB76F75635D8679BA43BC6444CBF9FD32CC337D9EBF482EEC0177C98B82EC2D77066F0F9EE84F2CA3C0CB298FC69FF67C99476568AB47AA43246444CC74217297D3141C59BD2A346DC3396C5EAD9D6C3B3385CBE87AF6F247A873C9893AA08629D4ACB93D1D9C5C882837";
            const string clientSecret = "4B5FD11989598BE3AD5539E1BB2BF37BE3A2D5C616B247DF864889245D05E4499E14BAA1139B875A8B65955C93C2379ED3BCBDA9F467C1C7196785B2CD1403969913484CD64909F6F55429B629093A11DBCB8B9DB9A43ACA38BA5B442F52030A1C492B15EB4222F1B11B943697BAA7A4562DB1C8289ABACDAE1C18D25C61FF7D2C68D958355237F3C1415DED412B2FBE88852D7B1815CBE4F4591C21BD9C826AE0966C87CCEA5E92E99934E5ECB96DB3858717185BFAD6CEBF3C7365F6E08F62FDFE38EE5C25E75169EBD31D5815693BAC83479674B4AEB652C732106FBEAB5FDCA5D771EE430596350B65BB9F6F2E4C89A5F292E479D36AF7DA1";
            var timeStamp = (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000;
            var methodPath = new Uri(testURL); //new Uri("http://localhost:16007/Api/Categories");

            var signature = getHMACMD5Signature(methodPath.LocalPath, "", timeStamp.ToString(CultureInfo.InvariantCulture), clientId, clientSecret);
            signature = HttpUtility.UrlEncode(signature);

            var request = WebRequest.Create(methodPath + string.Format("?clientid={0}&timestamp={1}&signature={2}", clientId, timeStamp, signature));


            request.Method = "POST";
            // Create POST data and convert it to a byte array.
            var postData = "UserId=3fe7b8f8-90e5-48e7-b747-94a767631253";
            var byteArray = Encoding.UTF8.GetBytes(postData);
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = byteArray.Length;
            var dataStream = request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();

            textBox3.Text = request.RequestUri.AbsoluteUri;

            var resp = request.GetResponse();
            var receiveStream = resp.GetResponseStream();
            if (receiveStream != null)
            {
                var readStream = new StreamReader(receiveStream, Encoding.UTF8);
                var returnValue = readStream.ReadToEnd();
                textBox1.Text = returnValue;

                resp.Close();
                readStream.Close();
            }    
        }

        private void button5_Click(object sender, EventArgs e)
        {
            const string testURL = "http://localhost:16007/Api/users/user/SaveUserFollowsEvent";


            const string clientId = "B65433C4E522F68117C9AB272E2797B3B87416C786847CC1EFFC6BEC63A7C402C77755056A2A452B935FA465F77932BD5CCEC807857978EA9C018FDA4C5B2CE2A1310A130B896154985D8D6D72DA56D36D73C91DF35B26A8F438FD2F13A1E28778E6EFAAFFF959C31316FD29A9E8AC2F31F5C1639678D2665831863EB47F33B7BE8C2A53BCD64E995AC15946C69CB76F75635D8679BA43BC6444CBF9FD32CC337D9EBF482EEC0177C98B82EC2D77066F0F9EE84F2CA3C0CB298FC69FF67C99476568AB47AA43246444CC74217297D3141C59BD2A346DC3396C5EAD9D6C3B3385CBE87AF6F247A873C9893AA08629D4ACB93D1D9C5C882837";
            const string clientSecret = "4B5FD11989598BE3AD5539E1BB2BF37BE3A2D5C616B247DF864889245D05E4499E14BAA1139B875A8B65955C93C2379ED3BCBDA9F467C1C7196785B2CD1403969913484CD64909F6F55429B629093A11DBCB8B9DB9A43ACA38BA5B442F52030A1C492B15EB4222F1B11B943697BAA7A4562DB1C8289ABACDAE1C18D25C61FF7D2C68D958355237F3C1415DED412B2FBE88852D7B1815CBE4F4591C21BD9C826AE0966C87CCEA5E92E99934E5ECB96DB3858717185BFAD6CEBF3C7365F6E08F62FDFE38EE5C25E75169EBD31D5815693BAC83479674B4AEB652C732106FBEAB5FDCA5D771EE430596350B65BB9F6F2E4C89A5F292E479D36AF7DA1";
            var timeStamp = (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000;
            var methodPath = new Uri(testURL);

            var signature = getHMACMD5Signature(methodPath.LocalPath, "", timeStamp.ToString(CultureInfo.InvariantCulture), clientId, clientSecret);
            signature = HttpUtility.UrlEncode(signature);

            var request = WebRequest.Create(methodPath + string.Format("?clientid={0}&timestamp={1}&signature={2}", clientId, timeStamp, signature));


            request.Method = "POST";
            // Create POST data and convert it to a byte array.
            var postData = "UserId=AFA23475-0971-4795-BDDC-70F5437150FE&EventId=133&Timestamp=" + DateTime.UtcNow;
            var byteArray = Encoding.UTF8.GetBytes(postData);
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = byteArray.Length;
            var dataStream = request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();

            textBox3.Text = request.RequestUri.AbsoluteUri;

            var resp = request.GetResponse();
            var receiveStream = resp.GetResponseStream();
            if (receiveStream != null)
            {
                var readStream = new StreamReader(receiveStream, Encoding.UTF8);
                var returnValue = readStream.ReadToEnd();
                textBox1.Text = returnValue;

                resp.Close();
                readStream.Close();
            }    
        }


    }
}
