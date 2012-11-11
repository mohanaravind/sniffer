using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

using Sniffer.Entity;

namespace Sniffer.Code
{
    class DBUtility
    {
        String _strIPAddress = "127.0.0.1";
        String _strPort = "27017";

        /// <summary>
        /// Method to get the Mongo Server reference
        /// </summary>
        /// <returns>MongoServer</returns>
        private MongoServer GetServer()
        {
            //Declarations
            StringBuilder sbConnectionString = new StringBuilder();
            MongoServer objMongoServer = null;

            try
            {
                //Construct the connection string
                sbConnectionString.Append("mongodb://");
                sbConnectionString.Append(_strIPAddress);
                sbConnectionString.Append(":");
                sbConnectionString.Append(_strPort);
                sbConnectionString.Append("/?safe=true");

                objMongoServer = MongoServer.Create(sbConnectionString.ToString());
            }
            catch (Exception ex)
            {
                Utility.DisplayException("GetServer", ex);
            }


            return objMongoServer;
        }                  

        /// <summary>
        /// Adds the Data base
        /// </summary>
        /// <param name="strDBName"></param>
        /// <returns></returns>
        public Boolean AddData(String strDBName, String strTableName, NetworkData objData)
        {
            //Declarations
            Boolean blnFlag = false;
            MongoDatabase objDB = GetServer().GetDatabase(strDBName);

            try
            {
                //Get the table
                MongoCollection objTable = objDB.GetCollection(strTableName);

                //Insert the data
                objTable.Insert(objData);
                objTable.Save(objData);
                blnFlag = true;
            }
            catch (Exception ex)
            {
                Utility.DisplayException("AddData", ex);
            }

            return blnFlag;
        }
    }
}
