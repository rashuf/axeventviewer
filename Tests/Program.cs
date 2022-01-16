using AxService;
using Model;
using System;
using System.Collections.Generic;


namespace Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            TextAxService();
            Console.ReadKey();
        }

        static void TextAxService()
        {
            UserContext         userContext = new UserContext("<domain>", "<username>", "<password>");

            List<EventInbox>   events;


            using (AxEventService service = new AxEventService(userContext))
            {
                events = service.GetList(QueryCriterias.UserNotRead("iau"));

                List<EventInbox> eventsForDel = new List<EventInbox>();

                foreach (var item in events)
                {
                    eventsForDel.Add(item);
                    break;
                }
                service.DeleteInboxes(eventsForDel);
            }
            Console.WriteLine(events.Count);
        }
    }
}
