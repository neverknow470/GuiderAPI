using System;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Api
{
    [RoutePrefix("api/getmsg")]
    public class GetmsgController : ApiController
    {
        string connStr = "server=localhost;uid=root;pwd=1234;database=guiderapidb";
        public class ReturnMsg
        {
            public string STATUS { get; set; }
            public string CARDID { get; set; }
            public string USERNAME { get; set; }
            public string MESSAGE { get; set; }
        }
        public class USERINFO
        {
            public string CARDID { get; set; }
            public string USERNAME { get; set; }
            public string ID { get; set; }
            public string PASSWORD { get; set; }
            public string BIRTHDAY { get; set; }
            public string GENDER { get; set; }
            public string ISSUEDDATE { get; set; }
            public string PHONE { get; set; }
            public string ECONTACT { get; set; }
            public string EPHONE { get; set; }
            public string STATUS { get; set; }
            public string MESSAGE { get; set; }
        }
        public class BODYINFO
        {
            public string CARDID { get; set; }
            public string INFOTYPE { get; set; }
            public string TYPEVALUE { get; set; }

        }
        public class BLOODPRESSURE
        {
            public string CARDID { get; set; }
            public string DBP { get; set; }
            public string SBP { get; set; }
            public string HB { get; set; }
            public string PU { get; set; }
        }
        public class BLOODSUGAR
        {
            public string CARDID { get; set; }
            public string BS { get; set; }
        }
        public class BODYTEMP
        {
            public string CARDID { get; set; }
            public string BT { get; set; }
        }
        public class BLOODOXYGEN
        {
            public string CARDID { get; set; }
            public string BO { get; set; }
        }
        public class SearchCondition
        {
            public string CARDID { get; set; }
            public string BODYTYPE { get; set; }
            public string SDATE { get; set; }
            public string EDATE { get; set; }
        }


        [HttpGet]
        public string SendCardId(string CARDID)
        {
            ReturnMsg msg = new ReturnMsg
            {
                STATUS = "",
                CARDID = CARDID,
                USERNAME = "使用者",
                MESSAGE = ""
            };
            try
            {
                MySqlConnection conn = new MySqlConnection(connStr);
                MySqlCommand command = conn.CreateCommand();
                conn.Open();
                command.CommandText = "SELECT * FROM CARDLIST WHERE CARDID='" + CARDID + "'";
                DataTable DT = new DataTable();
                MySqlDataAdapter MDA = new MySqlDataAdapter(command.CommandText, conn);
                MDA.Fill(DT);
                if (DT != null && DT.Rows.Count > 0)
                {
                    if (DT.Rows[0]["USERID"].ToString() != "")
                    {
                        command.CommandText = "SELECT * FROM USERINFO WHERE USERID='" + DT.Rows[0]["USERID"].ToString() + "'";
                        DT = new DataTable();
                        MDA = new MySqlDataAdapter(command.CommandText, conn);
                        MDA.Fill(DT);
                        if (DT != null && DT.Rows.Count > 0) CARDID = DT.Rows[0]["USERNAME"].ToString();
                    }
                }
                else
                {
                    command.CommandText = "INSERT INTO CARDLIST(CARDID,CDATE,LDATE) values('" + CARDID + "','" + DateTime.Today.ToString("yyyyMMdd") + "',NOW())";
                    command.ExecuteNonQuery();
                }
                conn.Close();
                msg.STATUS = "200";
                msg.MESSAGE = "INSERT SUCCESSFUL";
            }
            catch (Exception EX)
            {
                msg.STATUS = "500";
                msg.MESSAGE = EX.Message;
            }
            return JsonConvert.SerializeObject(msg, Formatting.None);
        }
        [HttpPost]
        public HttpResponseMessage SendUserInfo(JObject json)
        {
            ReturnMsg msg = new ReturnMsg
            {
                STATUS = "",
                CARDID = "",
                USERNAME = "",
                MESSAGE = ""
            };
            try
            {
                MySqlConnection conn = new MySqlConnection(connStr);
                MySqlCommand command = conn.CreateCommand();
                USERINFO user = new USERINFO
                {
                    CARDID = json["CARDID"].ToString(),
                    USERNAME = json["USERNAME"].ToString(),
                    ID = json["ID"].ToString(),
                    PASSWORD = json["BIRTHDAY"].ToString().Replace("/", "").Replace("-", ""),
                    BIRTHDAY = json["BIRTHDAY"].ToString(),
                    GENDER = json["GENDER"].ToString(),
                    ISSUEDDATE = json["ISSUEDDATE"].ToString(),
                    //ECONTACT = json["ECONTACT"].ToString()
                };//{"CARDID":"223456789","USERNAME":"ABC543","ID":"F123456789","BIRTHDAY":"19851029","GENDER":"M","ISSUEDDATE":"2018/08/22"}
                msg.CARDID = user.CARDID;
                msg.USERNAME = user.USERNAME;
                conn.Open();
                command.CommandText = "SELECT * FROM CARDLIST WHERE CARDID='" + user.CARDID + "'";
                DataTable DT = new DataTable();
                MySqlDataAdapter MDA = new MySqlDataAdapter(command.CommandText, conn);
                MDA.Fill(DT);
                if (DT != null && DT.Rows.Count > 0)
                {
                    if (DT.Rows[0]["USERID"].ToString() != "")
                    {
                        command.CommandText = "SELECT * FROM USERINFO WHERE USERID='" + DT.Rows[0]["USERID"].ToString() + "'";
                        DT = new DataTable();
                        MDA = new MySqlDataAdapter(command.CommandText, conn);
                        MDA.Fill(DT);
                        if (DT != null && DT.Rows.Count > 0)
                        {
                            msg.USERNAME = DT.Rows[0]["USERNAME"].ToString();
                            msg.MESSAGE = "EXIST";
                        }
                        else
                        {
                            string USERID = GetUSERID();
                            command.CommandText = @"INSERT INTO USERINFO(USERID,USERNAME,ID,PASSWORD,BIRTHDAY,GENDER,ISSUEDDATE,CDATE,MDATE,STATUS) 
                            values('" + USERID + "','" + user.USERNAME + "','" + user.ID + "','" + user.PASSWORD + "','" + user.BIRTHDAY + "','" + user.GENDER + "','" + user.ISSUEDDATE + "',NOW(),NOW(),1)";
                            command.ExecuteNonQuery();
                            command.CommandText = "UPDATE CARDLIST SET USERID='" + USERID + "' WHERE CARDID='" + user.CARDID + "'";
                            command.ExecuteNonQuery();
                            msg.MESSAGE = "INSERT SUCCESSFUL";
                        }
                    }
                }
                else
                {
                    string USERID = GetUSERID();
                    command.CommandText = "INSERT INTO CARDLIST(CARDID,USERID,CDATE,LDATE) values('" + user.CARDID + "','" + USERID + "','" + DateTime.Today.ToString("yyyyMMdd") + "',NOW())";
                    command.ExecuteNonQuery();
                    command.CommandText = @"INSERT INTO USERINFO(USERID,USERNAME,ID,PASSWORD,BIRTHDAY,GENDER,ISSUEDDATE,CDATE,MDATE,STATUS) 
                    values('" + USERID + "','" + user.USERNAME + "','" + user.ID + "','" + user.PASSWORD + "','" + user.BIRTHDAY + "','" + user.GENDER + "','" + user.ISSUEDDATE + "',NOW(),NOW(),1)";
                    command.ExecuteNonQuery();
                    msg.MESSAGE = "INSERT SUCCESSFUL";
                }
                conn.Close();
                msg.STATUS = "200";
            }
            catch (Exception EX)
            {
                msg.STATUS = "500";
                msg.MESSAGE = EX.Message;
            }
            return Request.CreateResponse(JsonConvert.SerializeObject(msg, Formatting.None));
        }
        private string GetUSERID()
        {
            string USERID = "U00000000000001";
            MySqlConnection conn = new MySqlConnection(connStr);
            MySqlCommand command = conn.CreateCommand();
            conn.Open();
            command.CommandText = "SELECT MAX(USERID) FROM USERINFO";
            DataTable DT = new DataTable();
            MySqlDataAdapter MDA = new MySqlDataAdapter(command.CommandText, conn);
            MDA.Fill(DT);
            if (DT != null && DT.Rows.Count > 0 && DT.Rows[0][0].ToString() != "")
            {
                int ID = int.Parse(DT.Rows[0][0].ToString().Replace("U", ""));
                USERID = "U" + (ID + 1).ToString().PadLeft(14, '0');
            }
            return USERID;
        }
        [HttpPost]
        public HttpResponseMessage SendBodyInfo(JObject json)
        {
            ReturnMsg msg = new ReturnMsg
            {
                STATUS = "",
                CARDID = "",
                USERNAME = "",
                MESSAGE = ""
            };
            try
            {
                //JObject json = JObject.Parse(STRJSON);
                MySqlConnection conn = new MySqlConnection(connStr);
                MySqlCommand command = conn.CreateCommand();
                BODYINFO body = new BODYINFO
                {
                    CARDID = json["CARDID"].ToString(),
                    INFOTYPE = json["INFOTYPE"].ToString(),
                    TYPEVALUE = json["TYPEVALUE"].ToString()
                };//{"CARDID":"123456789","INFOTYPE":"BPS","TYPEVALUE":"60"} {"CARDID":"223456789","INFOTYPE":"BPS","TYPEVALUE":"75"}
                msg.CARDID = body.CARDID;
                conn.Open();
                command.CommandText = "select * from cardlist C left join userinfo U on C.USERID=U.USERID WHERE CARDID='" + body.CARDID + "'";
                DataTable DT = new DataTable();
                DataTable DTb = new DataTable();
                MySqlDataAdapter MDA = new MySqlDataAdapter(command.CommandText, conn);
                MDA.Fill(DT);
                if (DT != null && DT.Rows.Count > 0 && DT.Rows[0]["USERID"].ToString() != "")
                {
                    msg.USERNAME = DT.Rows[0]["USERNAME"].ToString() == "" ? "使用者" : DT.Rows[0]["USERNAME"].ToString();
                    command.CommandText = "SELECT * FROM bodyinfo WHERE USERID='" + DT.Rows[0]["USERID"].ToString() + "' AND INFOTYPE='" + body.INFOTYPE + "'";
                    MDA = new MySqlDataAdapter(command.CommandText, conn);
                    MDA.Fill(DTb);
                    if (DTb != null && DTb.Rows.Count > 0 && DTb.Rows[0]["USERID"].ToString() != "")
                    {
                        command.CommandText = "UPDATE bodyinfo SET TYPEVALUE=" + body.TYPEVALUE + ",MDATE=NOW() WHERE USERID='" + DT.Rows[0]["USERID"].ToString() + "' AND INFOTYPE='" + body.INFOTYPE + "'";
                        msg.MESSAGE = "UPDATE SUCCESSFUL";
                    }
                    else
                    {
                        command.CommandText = "INSERT INTO bodyinfo(USERID,INFOTYPE,TYPEVALUE,MDATE) values('" + DT.Rows[0]["USERID"].ToString() + "','" + body.INFOTYPE + "'," + body.TYPEVALUE + ",NOW())";
                        msg.MESSAGE = "INSERT SUCCESSFUL";
                    }
                    command.ExecuteNonQuery();
                }
                else
                {
                    command.CommandText = "SELECT * FROM bodyinfo WHERE USERID='" + body.CARDID + "' AND INFOTYPE='" + body.INFOTYPE + "'";
                    MDA = new MySqlDataAdapter(command.CommandText, conn);
                    MDA.Fill(DTb);
                    if (DTb != null && DTb.Rows.Count > 0 && DTb.Rows[0]["USERID"].ToString() != "")
                    {
                        command.CommandText = "UPDATE bodyinfo SET TYPEVALUE=" + body.TYPEVALUE + ",MDATE=NOW() WHERE USERID='" + body.CARDID + "' AND INFOTYPE='" + body.INFOTYPE + "'";
                        msg.MESSAGE = "UPDATE SUCCESSFUL";
                    }
                    else
                    {
                        command.CommandText = "INSERT INTO bodyinfo(USERID,INFOTYPE,TYPEVALUE,MDATE) values('" + body.CARDID + "','" + body.INFOTYPE + "'," + body.TYPEVALUE + ",NOW())";
                        msg.MESSAGE = "INSERT SUCCESSFUL";
                    }
                    command.ExecuteNonQuery();
                }
                conn.Close();
                msg.STATUS = "200";
            }
            catch (Exception EX)
            {
                msg.STATUS = "500";
                msg.MESSAGE = EX.Message;
            }
            return Request.CreateResponse(JsonConvert.SerializeObject(msg, Formatting.None));
        }

        [HttpPost]
        public HttpResponseMessage SendBPInfo(JObject json)
        {
            ReturnMsg msg = new ReturnMsg
            {
                STATUS = "",
                CARDID = "",
                USERNAME = "",
                MESSAGE = ""
            };
            try
            {
                MySqlConnection conn = new MySqlConnection(connStr);
                MySqlCommand command = conn.CreateCommand();
                BLOODPRESSURE BLOODPRESSURE = new BLOODPRESSURE
                {
                    CARDID = json["CARDID"].ToString(),
                    DBP = json["DBP"].ToString(),
                    SBP = json["SBP"].ToString(),
                    HB = json["HB"].ToString(),
                    PU = json["PU"].ToString()
                };
                msg.CARDID = BLOODPRESSURE.CARDID;
                conn.Open();
                command.CommandText = "select * from cardlist C left join userinfo U on C.USERID=U.USERID WHERE CARDID='" + BLOODPRESSURE.CARDID + "'";
                DataTable DT = new DataTable();
                DataTable DTb = new DataTable();
                MySqlDataAdapter MDA = new MySqlDataAdapter(command.CommandText, conn);
                MDA.Fill(DT);
                if (DT != null && DT.Rows.Count > 0 && DT.Rows[0]["USERID"].ToString() != "")
                {
                    msg.USERNAME = DT.Rows[0]["USERNAME"].ToString() == "" ? "使用者" : DT.Rows[0]["USERNAME"].ToString();
                    command.CommandText = "INSERT INTO BLOODPRESSURE(USERID,DBP,SBP,HB,PU,CDATE) values('" + DT.Rows[0]["USERID"].ToString() + "'," + BLOODPRESSURE.DBP + "," + BLOODPRESSURE.SBP + "," + BLOODPRESSURE.HB + "," + BLOODPRESSURE.PU + ",NOW())";
                    msg.MESSAGE = "INSERT SUCCESSFUL";
                    command.ExecuteNonQuery();
                }
                else
                {

                    command.CommandText = "INSERT INTO BLOODPRESSURE(USERID,DBP,SBP,HB,PU,CDATE) values('" + BLOODPRESSURE.CARDID + "'," + BLOODPRESSURE.DBP + "," + BLOODPRESSURE.SBP + "," + BLOODPRESSURE.HB + "," + BLOODPRESSURE.PU + ",NOW())";
                    msg.MESSAGE = "INSERT SUCCESSFUL";
                    command.ExecuteNonQuery();
                }
                conn.Close();
                msg.STATUS = "200";
            }
            catch (Exception EX)
            {
                msg.STATUS = "500";
                msg.MESSAGE = EX.Message;
            }
            return Request.CreateResponse(JsonConvert.SerializeObject(msg, Formatting.None));
        }

        [HttpPost]
        public HttpResponseMessage SendBSInfo(JObject json)
        {
            ReturnMsg msg = new ReturnMsg
            {
                STATUS = "",
                CARDID = "",
                USERNAME = "",
                MESSAGE = ""
            };
            try
            {
                MySqlConnection conn = new MySqlConnection(connStr);
                MySqlCommand command = conn.CreateCommand();
                BLOODSUGAR BLOODSUGAR = new BLOODSUGAR
                {
                    CARDID = json["CARDID"].ToString(),
                    BS = json["BS"].ToString()
                };
                msg.CARDID = BLOODSUGAR.CARDID;
                conn.Open();
                command.CommandText = "select * from cardlist C left join userinfo U on C.USERID=U.USERID WHERE CARDID='" + BLOODSUGAR.CARDID + "'";
                DataTable DT = new DataTable();
                DataTable DTb = new DataTable();
                MySqlDataAdapter MDA = new MySqlDataAdapter(command.CommandText, conn);
                MDA.Fill(DT);
                if (DT != null && DT.Rows.Count > 0 && DT.Rows[0]["USERID"].ToString() != "")
                {
                    msg.USERNAME = DT.Rows[0]["USERNAME"].ToString() == "" ? "使用者" : DT.Rows[0]["USERNAME"].ToString();
                    command.CommandText = "INSERT INTO BLOODSUGAR(USERID,BS,CDATE) values('" + DT.Rows[0]["USERID"].ToString() + "'," + BLOODSUGAR.BS + ",NOW())";
                    msg.MESSAGE = "INSERT SUCCESSFUL";
                    command.ExecuteNonQuery();
                }
                else
                {

                    command.CommandText = "INSERT INTO BLOODSUGAR(USERID,BS,CDATE) values('" + BLOODSUGAR.CARDID + "'," + BLOODSUGAR.BS + ",NOW())";
                    msg.MESSAGE = "INSERT SUCCESSFUL";
                    command.ExecuteNonQuery();
                }
                conn.Close();
                msg.STATUS = "200";
            }
            catch (Exception EX)
            {
                msg.STATUS = "500";
                msg.MESSAGE = EX.Message;
            }
            return Request.CreateResponse(JsonConvert.SerializeObject(msg, Formatting.None));
        }

        [HttpPost]
        public HttpResponseMessage SendBTInfo(JObject json)
        {
            ReturnMsg msg = new ReturnMsg
            {
                STATUS = "",
                CARDID = "",
                USERNAME = "",
                MESSAGE = ""
            };
            try
            {
                MySqlConnection conn = new MySqlConnection(connStr);
                MySqlCommand command = conn.CreateCommand();
                BODYTEMP BODYTEMP = new BODYTEMP
                {
                    CARDID = json["CARDID"].ToString(),
                    BT = json["BT"].ToString()
                };
                msg.CARDID = BODYTEMP.CARDID;
                conn.Open();
                command.CommandText = "select * from cardlist C left join userinfo U on C.USERID=U.USERID WHERE CARDID='" + BODYTEMP.CARDID + "'";
                DataTable DT = new DataTable();
                DataTable DTb = new DataTable();
                MySqlDataAdapter MDA = new MySqlDataAdapter(command.CommandText, conn);
                MDA.Fill(DT);
                if (DT != null && DT.Rows.Count > 0 && DT.Rows[0]["USERID"].ToString() != "")
                {
                    msg.USERNAME = DT.Rows[0]["USERNAME"].ToString() == "" ? "使用者" : DT.Rows[0]["USERNAME"].ToString();
                    command.CommandText = "INSERT INTO BODYTEMP(USERID,BT,CDATE) values('" + DT.Rows[0]["USERID"].ToString() + "'," + BODYTEMP.BT + ",NOW())";
                    msg.MESSAGE = "INSERT SUCCESSFUL";
                    command.ExecuteNonQuery();
                }
                else
                {

                    command.CommandText = "INSERT INTO BODYTEMP(USERID,BT,CDATE) values('" + BODYTEMP.CARDID + "'," + BODYTEMP.BT + ",NOW())";
                    msg.MESSAGE = "INSERT SUCCESSFUL";
                    command.ExecuteNonQuery();
                }
                conn.Close();
                msg.STATUS = "200";
            }
            catch (Exception EX)
            {
                msg.STATUS = "500";
                msg.MESSAGE = EX.Message;
            }
            return Request.CreateResponse(JsonConvert.SerializeObject(msg, Formatting.None));
        }

        [HttpPost]
        public HttpResponseMessage SendBOInfo(JObject json)
        {
            ReturnMsg msg = new ReturnMsg
            {
                STATUS = "",
                CARDID = "",
                USERNAME = "",
                MESSAGE = ""
            };
            try
            {
                MySqlConnection conn = new MySqlConnection(connStr);
                MySqlCommand command = conn.CreateCommand();
                BLOODOXYGEN BLOODOXYGEN = new BLOODOXYGEN
                {
                    CARDID = json["CARDID"].ToString(),
                    BO = json["BO"].ToString()
                };
                msg.CARDID = BLOODOXYGEN.CARDID;
                conn.Open();
                command.CommandText = "select * from cardlist C left join userinfo U on C.USERID=U.USERID WHERE CARDID='" + BLOODOXYGEN.CARDID + "'";
                DataTable DT = new DataTable();
                DataTable DTb = new DataTable();
                MySqlDataAdapter MDA = new MySqlDataAdapter(command.CommandText, conn);
                MDA.Fill(DT);
                if (DT != null && DT.Rows.Count > 0 && DT.Rows[0]["USERID"].ToString() != "")
                {
                    msg.USERNAME = DT.Rows[0]["USERNAME"].ToString() == "" ? "使用者" : DT.Rows[0]["USERNAME"].ToString();
                    command.CommandText = "INSERT INTO BLOODOXYGEN(USERID,BO,CDATE) values('" + DT.Rows[0]["USERID"].ToString() + "'," + BLOODOXYGEN.BO + ",NOW())";
                    msg.MESSAGE = "INSERT SUCCESSFUL";
                    command.ExecuteNonQuery();
                }
                else
                {
                    command.CommandText = "INSERT INTO BLOODOXYGEN(USERID,BO,CDATE) values('" + BLOODOXYGEN.CARDID + "'," + BLOODOXYGEN.BO + ",NOW())";
                    msg.MESSAGE = "INSERT SUCCESSFUL";
                    command.ExecuteNonQuery();
                }
                conn.Close();
                msg.STATUS = "200";
            }
            catch (Exception EX)
            {
                msg.STATUS = "500";
                msg.MESSAGE = EX.Message;
            }
            return Request.CreateResponse(JsonConvert.SerializeObject(msg, Formatting.None));
        }

        [HttpGet]
        public HttpResponseMessage GetUserInfo(string CARDID)
        {
            USERINFO userinfo = new USERINFO
            {
                CARDID = CARDID,
                USERNAME = "使用者",
                ID = "",
                BIRTHDAY = "",
                GENDER = "",
                ISSUEDDATE = "",
                PHONE = "",
                ECONTACT = "",
                EPHONE = "",
                STATUS = "200",
                MESSAGE = "Search Success"
            };
            try
            {
                MySqlConnection conn = new MySqlConnection(connStr);
                MySqlCommand command = conn.CreateCommand();
                conn.Open();
                command.CommandText = "select * from cardlist C left join userinfo U on C.USERID=U.USERID WHERE CARDID='" + CARDID + "'";
                DataTable DT = new DataTable();
                MySqlDataAdapter MDA = new MySqlDataAdapter(command.CommandText, conn);
                MDA.Fill(DT);
                if (DT != null && DT.Rows.Count > 0 && DT.Rows[0]["USERID"].ToString() != "")
                {
                    userinfo.USERNAME = DT.Rows[0]["USERNAME"].ToString();
                    userinfo.ID = DT.Rows[0]["ID"].ToString();
                    userinfo.BIRTHDAY = DT.Rows[0]["BIRTHDAY"].ToString();
                    userinfo.GENDER = DT.Rows[0]["GENDER"].ToString();
                    userinfo.ISSUEDDATE = DT.Rows[0]["ISSUEDDATE"].ToString();
                    userinfo.PHONE = DT.Rows[0]["PHONE"].ToString();
                    userinfo.ECONTACT = DT.Rows[0]["ECONTACT"].ToString();
                    userinfo.EPHONE = DT.Rows[0]["EPHONE"].ToString();
                }
                conn.Close();
            }
            catch (Exception EX)
            {
                userinfo.STATUS = "500";
                userinfo.MESSAGE = EX.Message;
            }
            return Request.CreateResponse(JsonConvert.SerializeObject(userinfo, Formatting.None));
        }

        [HttpPost]
        public HttpResponseMessage GetBodyInfo(JObject json)
        {
            SearchCondition sc = new SearchCondition
            {
                CARDID = json["CARDID"].ToString(),
                BODYTYPE = json["BODYTYPE"].ToString(),
                SDATE = json["SDATE"].ToString(),
                EDATE = json["EDATE"].ToString()
            };

            dynamic bodyinfo = new JObject();
            bodyinfo.CARDID = json["CARDID"].ToString();
            bodyinfo.STATUS = "200";
            bodyinfo.MESSAGE = "Search Success";
            try
            {
                MySqlConnection conn = new MySqlConnection(connStr);
                MySqlCommand command = conn.CreateCommand();
                conn.Open();
                command.CommandText = @"select B.* from cardlist C left join userinfo U on C.USERID=U.USERID 
                left join " + sc.BODYTYPE + @" B on B.USERID=CASE WHEN C.USERID is null OR C.USERID='' THEN C.CARDID ELSE C.USERID END
                WHERE CARDID='" + sc.CARDID + "'";
                if (sc.SDATE != "") command.CommandText += " AND B.CDATE>='" + sc.SDATE + "'";
                if (sc.EDATE != "") command.CommandText += " AND B.CDATE<='" + sc.EDATE + " 23:59:59'";
                DataTable DT = new DataTable();
                MySqlDataAdapter MDA = new MySqlDataAdapter(command.CommandText, conn);
                MDA.Fill(DT);
                if (DT != null && DT.Rows.Count > 0 && DT.Rows[0]["USERID"].ToString() != "")
                {
                    DT.Columns.Remove("USERID");
                    bodyinfo.BODY = JsonConvert.SerializeObject(DT, Formatting.None);
                }
                conn.Close();
            }
            catch (Exception EX)
            {
                bodyinfo.STATUS = "500";
                bodyinfo.MESSAGE = EX.Message;
            }
            string Result = bodyinfo.ToString(Formatting.None);
            return Request.CreateResponse(Result);
        }
    }
}

