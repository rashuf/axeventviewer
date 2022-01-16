using Model.EventInboxService;
using System;

namespace AxService
{
    public static class QueryCriterias
    {
        public static QueryCriteria UserNotRead(string userId)
        {
            CriteriaElement[] element =
            {
                new CriteriaElement()
                {
                    DataSourceName = "EventInbox",
                    FieldName = "UserId",
                    Operator = Operator.Equal,
                    Value1 = userId
                }
                ,
                new CriteriaElement()
                {
                    DataSourceName = "EventInbox",
                    FieldName = "IsRead",
                    Operator = Operator.Equal,
                    Value1 = "No"
                }
            };
            QueryCriteria query = new QueryCriteria();
            query.CriteriaElement = element;

            return query;
        }
        public static QueryCriteria UserNotReadEventId(string userId, long? inboxId)
        {
            CriteriaElement[] element =
            {
                new CriteriaElement()
                {
                    DataSourceName = "EventInbox",
                    FieldName = "UserId",
                    Operator = Operator.Equal,
                    Value1 = userId
                }
                ,
                new CriteriaElement()
                {
                    DataSourceName = "EventInbox",
                    FieldName = "IsRead",
                    Operator = Operator.Equal,
                    Value1 = "No"
                }
                ,
                new CriteriaElement()
                {
                    DataSourceName = "EventInbox",
                    FieldName = "InboxId",
                    Operator = Operator.Greater,
                    Value1 = inboxId.ToString()
                },
                new CriteriaElement()
                {
                    DataSourceName = "EventInbox",
                    FieldName = "Visible",
                    Operator = Operator.Equal,
                    Value1 = "Yes"
                },

                new CriteriaElement()
                {
                    DataSourceName = "EventInbox",
                    FieldName = "Deleted",
                    Operator = Operator.Equal,
                    Value1 = "No"
                }
            };

            QueryCriteria query = new QueryCriteria();
            query.CriteriaElement = element;

            return query;
        }
        public static QueryCriteria UserIsReadEventId(string userId, long? inboxId)
        {
            CriteriaElement[] element =
            {
                new CriteriaElement()
                {
                    DataSourceName = "EventInbox",
                    FieldName = "UserId",
                    Operator = Operator.Equal,
                    Value1 = userId
                }
                ,
                new CriteriaElement()
                {
                    DataSourceName = "EventInbox",
                    FieldName = "IsRead",
                    Operator = Operator.Equal,
                    Value1 = "1"
                }
                ,
                new CriteriaElement()
                {
                    DataSourceName = "EventInbox",
                    FieldName = "InboxId",
                    Operator = Operator.Equal,
                    Value1 = inboxId.ToString()
                }

            };

            QueryCriteria query = new QueryCriteria();
            query.CriteriaElement = element;

            return query;
        }

        public static QueryCriteria UserAllEvents(string userId)
        {
            CriteriaElement[] element =
            {
                new CriteriaElement()
                {
                    DataSourceName = "EventInbox",
                    FieldName = "UserId",
                    Operator = Operator.Equal,
                    Value1 = userId
                }
            };
            QueryCriteria query = new QueryCriteria();
            query.CriteriaElement = element;

            return query;
        }
        public static QueryCriteria FindByRecId(Int64 recId)
        {
            CriteriaElement[] element =
            {
                new CriteriaElement()
                {
                    DataSourceName = "EventInbox",
                    FieldName = "RecId",
                    Operator = Operator.Equal,
                    Value1 = recId.ToString()
                }
            };
            QueryCriteria query = new QueryCriteria();
            query.CriteriaElement = element;

            return query;
        }
        public static QueryCriteria UserNeedPopup(string userId)
        {
            CriteriaElement[] element =
             {
                new CriteriaElement()
                {
                    DataSourceName = "EventInbox",
                    FieldName = "UserId",
                    Operator = Operator.Equal,
                    Value1 = userId
                },
                new CriteriaElement()
                {
                    DataSourceName = "EventInbox",
                    FieldName = "IsRead",
                    Operator = Operator.Equal,
                    Value1 = "0"
                },
                new CriteriaElement()
                {
                    DataSourceName = "EventInbox",
                    FieldName = "ShowPopup",
                    Operator = Operator.Equal,
                    Value1 = "1"
                }
            };
            QueryCriteria query = new QueryCriteria();
            query.CriteriaElement = element;

            return query;
        }

    }
}
