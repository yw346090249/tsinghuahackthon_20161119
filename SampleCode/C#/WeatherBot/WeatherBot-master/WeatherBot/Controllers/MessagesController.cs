using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;

namespace WeatherBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                // calculate something for us to return
                string queryString = (activity.Text ?? string.Empty);
                string city = string.Empty;
                string replyMsg = string.Empty;

                var lData = await LuisService.ParseUserInput(queryString);

                if (lData != null)
                {
                    if (lData.intents.Count() > 1)
                    {
                        switch (lData.intents[0].intent)
                        {
                            case "GetWeather":
                                {
                                    if (lData.entities.Count() == 0)
                                    {
                                        replyMsg = "do not know where you are looking for?";
                                    }
                                    else if (lData.entities[0].type == "City")
                                    {
                                        city = lData.entities[0].entity;
                                        replyMsg = await GetWeather(city);
                                    }
                                    else
                                        replyMsg = "I do not understand your city...";

                                }
                                break;
                            default:
                                {
                                    replyMsg = "I can only check the weather now...";
                                    break;
                                }

                        }
                    }

                    else
                        replyMsg = "I do not understand your quesiton";
                       


                }
                else
                    replyMsg = "Sorry I do not understand, please speak English";

                // return our reply to the user
               
                Activity reply = activity.CreateReply(replyMsg);
                await connector.Conversations.ReplyToActivityAsync(reply);
            }
            else
            {
                HandleSystemMessage(activity);
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }
        private async Task<string> GetWeather(string strCity)
        {
            string strRet = string.Empty;
            string strTemp = string.Empty;

            WeatherData wd = new WeatherData();

            wd = await WeatherService.GetWeatherfromService(strCity);
            // return our reply to the user
            if (null == wd)
            {
                strRet = string.Format("City {0} doesn't appear to be valid", strCity.ToUpper());
            }
            else
            {
                strRet = string.Format(" {0} now, {1}: {2} and {3} Degree", wd.city, wd.weathermain, wd.weatherdescription, wd.temp);
            }
            return strRet;
        }

        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }
    }
}