using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Data;
using System.Net.Mail;

namespace ServerUDPRastroSegue
{
    class Program
    {        
        static void Main(string[] args)
        {
            Console.Title = ".:: Server RastroSegue UDP Version 2.59 ::.";            
            while (true)
            {
                try
                {
                    int recv;
                    byte[] data = new byte[1024];

                    IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 54321);

                    Socket newSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    newSocket.Bind(endPoint);

                    Console.WriteLine("Whating for a client ..");

                    IPEndPoint sender = new IPEndPoint(IPAddress.Any, 54321);
                    EndPoint tmpRemote = (EndPoint)sender;

                    recv = newSocket.ReceiveFrom(data, ref tmpRemote);

                    Console.WriteLine("Menssage receive from {0}", tmpRemote.ToString());

                    Console.WriteLine(Encoding.ASCII.GetString(data, 0, recv));                    

                    GravaLocalizacoes(System.Text.Encoding.Default.GetString(data));                    

                    newSocket.Close();
                }
                catch (Exception) { }                
            }
        }

        protected static void EnviaEmail(string mMensagem, string mDestino)
        {
            if (mDestino.TrimEnd() != string.Empty && mDestino.TrimEnd() != "-")
            {                
                string emailDestinatario = mDestino; // ************ buscar email do cliente na base de dados *************                    

                string assuntoMensagem = "Aviso RastroSegue";
                string conteudoMensagem = "*** SISTEMA DE MENSAGENS DO WWW.RASTROSEGUE.COM.BR **** <br /><br />"
                                        + mMensagem + "<br /><br />"
                                        + "===================================================================== <br />"
                                        + "BetaBlue - Desenvolvimento de Sistemas Customizados <br />"
                                        + "===================================================================== <br />"
                                        + "Em caso de dúvidas, entre em contato com um dos nossos canais de atendimento disponíveis no email: <br />"
                                        + "contato@rastrosegue.com.br <br /><br />"
                                        + "Eduardo Elias Alves Pereira <br />"
                                        + "Analista de Sistemas e Gerente de Desenvolvimento";

                MailMessage objEmail = new MailMessage();
                objEmail.From = new System.Net.Mail.MailAddress("RastroSegue - Suporte" + "<" + "contato@rastrosegue.com.br" + ">");
                objEmail.To.Add(emailDestinatario);
                objEmail.Priority = System.Net.Mail.MailPriority.Normal;
                objEmail.IsBodyHtml = true;
                objEmail.Subject = assuntoMensagem;
                objEmail.Body = conteudoMensagem;
                //objEmail.Bcc.Add("eduardo.eliasap@gmail.com");
                objEmail.SubjectEncoding = System.Text.Encoding.GetEncoding("ISO-8859-1");
                objEmail.BodyEncoding = System.Text.Encoding.GetEncoding("ISO-8859-1");

                System.Net.Mail.SmtpClient objSmtp = new System.Net.Mail.SmtpClient();

                objSmtp.Credentials = new System.Net.NetworkCredential("seu email", "sua senha");
                objSmtp.Host = "smtp.betablue.com.br";
                objSmtp.Port = 587;
                objSmtp.Send(objEmail);
            }
        }
        protected static string RetornaData(string mData)
        {
            string[] mVet = mData.Split(' ');
            return mVet[0];
        }
        protected static string AlteraData(string mData)
        {
            mData = RetornaData(mData);
            string[] mVet = mData.Split('/');
            return mVet[2] + "-" + mVet[1] + "-" + mVet[0];
        }
        protected static void GravaLocalizacoes(string texto)
        {
            Localizadores objLocalizadores = new Localizadores();
            try
            {
                string mNumChipLocalizador = string.Empty;
                string mNumTelChip = string.Empty;
                string mComando = string.Empty;
                string mLegenda = string.Empty;
                string mLegendaAviso = string.Empty;
                string mAviMensagem = string.Empty;
                string mEnderecoCompleto = string.Empty;
                string[] mVet = texto.Split('.');
                DataTable dt = null;
                mComando = string.Empty;

                texto = texto.Replace("\0","");

                if (texto.Substring(0, 1) == "#" && texto.Substring(1, 1) != "#")
                {
                    mVet = texto.Split('#');
                    dt = objLocalizadores.RetornaVeiCodigo(mVet[1].ToString());
                    if (dt == null && dt.Rows.Count == 0)
                    {                        
                        if (objLocalizadores.Gravar(mComando) == 1) { }
                    }
                    else
                        Console.WriteLine("Cliente login: " + dt.Rows[0]["Usu_Login"].ToString().TrimEnd() + " - " + dt.Rows[0]["Veiculo"].ToString());
                } // Familia TLT
                if (texto.Substring(0, 4) == "imei")
                {
                    mVet = texto.Split(',');
                    dt = objLocalizadores.RetornaVeiCodigo(mVet[0].ToString().Substring(5, 15));
                    if (dt.Rows.Count == 0)
                    {                        
                        Console.WriteLine("IMEI sem cadastro: " + mVet[0].ToString().Substring(5, 15));
                    }
                    else
                    {
                        Console.WriteLine("Cliente login: " + dt.Rows[0]["Usu_Login"].ToString().TrimEnd() + " - " + dt.Rows[0]["Veiculo"].ToString());
                        mNumChipLocalizador = dt.Rows[0]["Chip_NumTel"].ToString().TrimEnd();
                    }
                } // Familia TK

                if (dt != null && dt.Rows.Count > 0)
                {

                    if (dt.Rows[0]["Mod_Codigo"].ToString() == "2" || dt.Rows[0]["Mod_Codigo"].ToString() == "3" || dt.Rows[0]["Mod_Codigo"].ToString() == "7" || dt.Rows[0]["Mod_Codigo"].ToString() == "8" || dt.Rows[0]["Mod_Codigo"].ToString() == "10")
                    {
                        mComando = string.Empty;
                        mLegenda = string.Empty;
                        mAviMensagem = string.Empty;
                        string[] mVetPosicoes = mVet[0].Split(',');

                        int mProxCodHistorico = objLocalizadores.ProxCodSeqHistorico();

                        int mCodVeiculo = Convert.ToInt32(dt.Rows[0]["Vei_Codigo"].ToString());

                        mComando = mComando + "|Insert Into Tab_Respostas (Vei_Codigo, Res_Mensagem, Res_Data, Res_Hora) Values ("
                                                + mCodVeiculo + ", '" + texto + "', '" + AlteraData(DateTime.Now.ToShortDateString()) + "', '" + DateTime.Now.ToShortTimeString() + "')";

                        string mLatitude = mVet[7].ToString();
                        string mLongitude = mVet[9].ToString();

                        if (mLatitude == "" || mLongitude == "") { }

                        string mLatGraus = mLatitude.Substring(0, mLatitude.IndexOf(".") - 2);
                        string mLatSegundos = (mLatitude.Substring(mLatitude.IndexOf(".") - 2, 2)).ToString() + mLatitude.Substring(mLatitude.IndexOf(".") + 1, mLatitude.Length - mLatitude.IndexOf(".") - 1);

                        string mLongGraus = mLongitude.Substring(0, mLongitude.IndexOf(".") - 2);
                        string mLongSegundos = (mLongitude.Substring(mLongitude.IndexOf(".") - 2, 2)).ToString() + mLongitude.Substring(mLongitude.IndexOf(".") + 1, mLongitude.Length - mLongitude.IndexOf(".") - 1);

                        string mAuxLatitude = (Convert.ToDouble("0," + mLatSegundos) / Convert.ToDouble("0,6")).ToString();

                        if (mAuxLatitude.Length > 7)
                            mAuxLatitude = mAuxLatitude.Substring(0, 7);

                        if (mAuxLatitude.Length == 7)
                            mLatitude = "-" + mLatGraus + "." + ((Convert.ToDouble("0," + mLatSegundos) / Convert.ToDouble("0,6")).ToString().Substring(2, 5));

                        if (mAuxLatitude.Length == 6)
                            mLatitude = "-" + mLatGraus + "." + ((Convert.ToDouble("0," + mLatSegundos) / Convert.ToDouble("0,6")).ToString().Substring(2, 4));

                        if (mAuxLatitude.Length == 5)
                            mLatitude = "-" + mLatGraus + "." + ((Convert.ToDouble("0," + mLatSegundos) / Convert.ToDouble("0,6")).ToString().Substring(2, 3)) + "0";

                        if (mAuxLatitude.Length == 4)
                            mLatitude = "-" + mLatGraus + "." + ((Convert.ToDouble("0," + mLatSegundos) / Convert.ToDouble("0,6")).ToString().Substring(2, 2)) + "00";

                        string mAuxLongitude = (Convert.ToDouble("0," + mLongSegundos) / Convert.ToDouble("0,6")).ToString();

                        if (mAuxLongitude.Length > 7)
                            mAuxLongitude = mAuxLongitude.Substring(0, 7);

                        if (mAuxLongitude.Length == 7)
                            mLongitude = "-" + mLongGraus + "." + ((Convert.ToDouble("0," + mLongSegundos) / Convert.ToDouble("0,6")).ToString().Substring(2, 5));

                        if (mAuxLongitude.Length == 6)
                            mLongitude = "-" + mLongGraus + "." + ((Convert.ToDouble("0," + mLongSegundos) / Convert.ToDouble("0,6")).ToString().Substring(2, 4));

                        if (mAuxLongitude.Length == 5)
                            mLongitude = "-" + mLongGraus + "." + ((Convert.ToDouble("0," + mLongSegundos) / Convert.ToDouble("0,6")).ToString().Substring(2, 3)) + "0";

                        if (mAuxLongitude.Length == 4)
                            mLongitude = "-" + mLongGraus + "." + ((Convert.ToDouble("0," + mLongSegundos) / Convert.ToDouble("0,6")).ToString().Substring(2, 2)) + "00";

                        string mVelocidade = "0";
                        if (mVet.Length == 11)
                            mVelocidade = objLocalizadores.RetornaKNOTSKM(mVet[11]);
                        if (mVelocidade.Length > 6)
                        {
                            mVelocidade = mVelocidade.Substring(0, 5);
                            if (Convert.ToDouble(mVelocidade.Replace(',', '.')) > 5)
                                mLegendaAviso = "Em movimento";
                            else
                                mLegendaAviso = "Parado";
                        }
                        else
                            mLegenda = "Parado";

                        string mData = DateTime.Now.ToShortDateString();
                        string[] mVetHora = DateTime.Now.AddHours(0).ToString().Split(' ');
                        string mHora = mVetHora[1];

                        if (mVet[1].ToString().TrimEnd() == "ac alarm")
                        {                            
                            /*** Verifica se ja houve um aviso a 30 minutos atrás ***/
                            if (objLocalizadores.TempoDeUltimoCorte(dt.Rows[0]["Usu_Telefone"].ToString().TrimEnd(), "Corte") > 720)
                            {
                                mNumTelChip = dt.Rows[0]["Usu_Telefone"].ToString().TrimEnd();
                                string mOperadora = string.Empty;
                                string mMensagem = "Corte de bateria do veículo: " + dt.Rows[0]["Veiculo"].ToString().TrimEnd();

                                EnviaEmail(mMensagem + " Cliente: " + dt.Rows[0]["Usu_NomeCompleto"].ToString().TrimEnd(), dt.Rows[0]["Usu_Email"].ToString().TrimEnd());                                

                                mOperadora = "2";                                                                
                                
                                mComando = mComando + "|Insert Into Tab_Avisos(Vei_Codigo, Avi_Mensagem, Avi_Latitude, Avi_Longitude, Avi_Status, Avi_Data, Avi_Hora, Avi_Legenda) Values ("
                                                    + mCodVeiculo.ToString() + ", '" + mMensagem + "', '" + mLatitude + "', '" + mLongitude + "', 'Pendente', '" + AlteraData(mData) + "', '" + mHora.Replace(" ", "") + "', '" + mLegendaAviso + "')";                                
                            }
                        }
                        if (mVet[1].ToString().TrimEnd() == "help me" || mVet[1].ToString().TrimEnd() == "help me!")
                        {
                            string mOperadora = string.Empty;

                            string mMensagem = "SOS do veiculo " + dt.Rows[0]["Veiculo"].ToString().TrimEnd() + ". rastroSegue.com.br Login: " + dt.Rows[0]["Usu_Login"].ToString().TrimEnd() + ", Senha: " + dt.Rows[0]["Usu_Senha"].ToString().TrimEnd();

                            DataTable dtSOS = objLocalizadores.RetornaDevices(Convert.ToInt32(dt.Rows[0]["Usu_Codigo"].ToString()));
                            if (dtSOS != null && dtSOS.Rows.Count > 0)
                            {
                                if (dtSOS.Rows.Count >= 1)
                                {
                                    mOperadora = dtSOS.Rows[0]["Dev_Operadora"].ToString().TrimEnd() == "VIVO" ? "2" : "1";
                                    mNumTelChip = dtSOS.Rows[0]["Dev_Numero"].ToString().TrimEnd();

                                    if (dtSOS.Rows[0]["Dev_Operadora"].ToString().TrimEnd() == "VIVO")
                                    {
                                        if (dtSOS.Rows[0]["Dev_Numero"].ToString().TrimEnd().Length == 13)
                                            mNumTelChip = "0" + dtSOS.Rows[0]["Dev_Numero"].ToString().TrimEnd().Substring(3, 10);
                                        if (dtSOS.Rows[0]["Dev_Numero"].ToString().TrimEnd().Length == 14)
                                            mNumTelChip = "0" + dtSOS.Rows[0]["Dev_numero"].ToString().TrimEnd().Substring(3, 11);
                                    }                                    
                                }
                                if (dtSOS.Rows.Count >= 2)
                                {
                                    mOperadora = dtSOS.Rows[1]["Dev_Operadora"].ToString().TrimEnd() == "VIVO" ? "2" : "1";
                                    mNumTelChip = dtSOS.Rows[0]["Dev_Numero"].ToString().TrimEnd();

                                    if (dtSOS.Rows[1]["Dev_Operadora"].ToString().TrimEnd() == "VIVO")
                                    {
                                        if (dtSOS.Rows[1]["Dev_Numero"].ToString().TrimEnd().Length == 13)
                                            mNumTelChip = "0" + dtSOS.Rows[1]["Dev_Numero"].ToString().TrimEnd().Substring(3, 10);
                                        if (dtSOS.Rows[1]["Dev_Numero"].ToString().TrimEnd().Length == 14)
                                            mNumTelChip = "0" + dtSOS.Rows[1]["Dev_numero"].ToString().TrimEnd().Substring(3, 11);
                                    }                                    
                                }
                                if (dtSOS.Rows.Count >= 3)
                                {
                                    mOperadora = dtSOS.Rows[2]["Dev_Operadora"].ToString().TrimEnd() == "VIVO" ? "2" : "1";
                                    mNumTelChip = dtSOS.Rows[0]["Dev_Numero"].ToString().TrimEnd();

                                    if (dtSOS.Rows[2]["Dev_Operadora"].ToString().TrimEnd() == "VIVO")
                                    {
                                        if (dtSOS.Rows[2]["Dev_Numero"].ToString().TrimEnd().Length == 13)
                                            mNumTelChip = "0" + dtSOS.Rows[2]["Dev_Numero"].ToString().TrimEnd().Substring(3, 10);
                                        if (dtSOS.Rows[2]["Dev_Numero"].ToString().TrimEnd().Length == 14)
                                            mNumTelChip = "0" + dtSOS.Rows[2]["Dev_numero"].ToString().TrimEnd().Substring(3, 11);
                                    }                                    
                                }
                                if (dtSOS.Rows.Count >= 4)
                                {
                                    mOperadora = dtSOS.Rows[3]["Dev_Operadora"].ToString().TrimEnd() == "VIVO" ? "2" : "1";
                                    mNumTelChip = dtSOS.Rows[3]["Dev_Numero"].ToString().TrimEnd();

                                    if (dtSOS.Rows[3]["Dev_Operadora"].ToString().TrimEnd() == "VIVO")
                                    {
                                        if (dtSOS.Rows[3]["Dev_Numero"].ToString().TrimEnd().Length == 13)
                                            mNumTelChip = "0" + dtSOS.Rows[3]["Dev_Numero"].ToString().TrimEnd().Substring(3, 10);
                                        if (dtSOS.Rows[3]["Dev_Numero"].ToString().TrimEnd().Length == 14)
                                            mNumTelChip = "0" + dtSOS.Rows[3]["Dev_numero"].ToString().TrimEnd().Substring(3, 11);
                                    }                                    
                                }
                                if (dtSOS.Rows.Count >= 5)
                                {
                                    mOperadora = dtSOS.Rows[4]["Dev_Operadora"].ToString().TrimEnd() == "VIVO" ? "2" : "1";
                                    mNumTelChip = dtSOS.Rows[4]["Dev_Numero"].ToString().TrimEnd();

                                    if (dtSOS.Rows[4]["Dev_Operadora"].ToString().TrimEnd() == "VIVO")
                                    {
                                        if (dtSOS.Rows[4]["Dev_Numero"].ToString().TrimEnd().Length == 13)
                                            mNumTelChip = "0" + dtSOS.Rows[4]["Dev_Numero"].ToString().TrimEnd().Substring(3, 10);
                                        if (dtSOS.Rows[4]["Dev_Numero"].ToString().TrimEnd().Length == 14)
                                            mNumTelChip = "0" + dtSOS.Rows[4]["Dev_numero"].ToString().TrimEnd().Substring(3, 11);
                                    }                                    
                                }                                
                            }
                        }
                        if ((Convert.ToInt32(Convert.ToDouble(mVelocidade.Replace('.', ','))) > Convert.ToInt32(dt.Rows[0]["Vei_SpeedLimit"].ToString())) && (Convert.ToInt32(dt.Rows[0]["Vei_SpeedLimit"].ToString()) != 0))                        
                        {
                            mLegenda = "M";
                            mAviMensagem = "Excesso de velocidade do veículo " + dt.Rows[0]["Veiculo"].ToString() + ": " + mVelocidade.TrimEnd();
                            mLegendaAviso = "Excesso de velocidade";

                            mNumTelChip = dt.Rows[0]["Usu_Telefone"].ToString().TrimEnd();

                            EnviaEmail(mAviMensagem, dt.Rows[0]["Usu_Email"].ToString().TrimEnd());
                            

                            mComando = mComando + "|Insert Into Tab_Avisos(Vei_Codigo, Avi_Mensagem, Avi_Latitude, Avi_Longitude, Avi_Status, Avi_Data, Avi_Hora, Avi_Legenda) Values ("
                                          + mCodVeiculo.ToString() + ", '" + mAviMensagem + "', '" + mLatitude + "', '" + mLongitude + "', 'Pendente', '" + AlteraData(mData) + "', '" + mHora.Replace(" ", "") + "', '" + mLegendaAviso + "')";                            
                        }
                        
                        if (dt.Rows[0]["Vei_SMSCerca"].ToString() == "S") // Cerca eletrônica
                        {                                                        
                            if (objLocalizadores.TempoDeUltimoCorte(dt.Rows[0]["Usu_Telefone"].ToString().TrimEnd(), "Cerca") > 720)
                            {
                                double mDistancCerca = objLocalizadores.RetornaDistanciaPontos(dt.Rows[0]["Vei_CercaLat"].ToString(), dt.Rows[0]["Vei_CercaLgn"].ToString(), mLatitude, mLongitude);                                

                                if (Convert.ToInt32(mDistancCerca) > Convert.ToDouble(dt.Rows[0]["Vei_CercaDistancMin"].ToString().TrimEnd()))
                                {
                                    EnviaEmail("Quebra de cerca eletrônica do veículo: " + dt.Rows[0]["Veiculo"].ToString().TrimEnd(), dt.Rows[0]["Usu_Email"].ToString().TrimEnd());                                    

                                    string mOperadora = string.Empty;
                                    string mMensagem = "Quebra de cerca eletrônica do veículo: " + dt.Rows[0]["Veiculo"].ToString().TrimEnd();
                                    mComando = mComando + "|Insert Into Tab_SolicitacoesHaver (Vei_NumTelChip, SoH_Status, SoH_Data, SoH_Hora, SoH_Mensagem, Ser_Codigo, SoH_Tipo) Values ('"
                                                        + dt.Rows[0]["Usu_Telefone"].ToString().TrimEnd() + "', 'Pendente', '" + DateTime.Now.ToShortDateString() + "', '" + DateTime.Now.ToLongTimeString() + "', '"
                                                        + mMensagem + "', '" + mOperadora + "', 'Cerca')";                                    
                                }                                
                            }
                        }
                        
                        mComando = mComando + "|Insert Into Tab_Historico(Vei_Codigo, His_Data, His_Hora, His_Latitude, His_Longitude, His_Speed, His_Status) Values ("
                                           + mCodVeiculo.ToString() + ", '" + objLocalizadores.AlteraData(mData) + "', '" + mHora + "', '"
                                           + mLatitude + "', '" + mLongitude + "', '" + mVelocidade + "', '" + mLegendaAviso + "')";
                        if (objLocalizadores.Gravar(mComando) == 1)
                        { }
                    }
                }
            }
            catch (Exception err)
            {
                //EnviaEmail(texto, "contato@rastrosegue.com.br"); 
                Console.WriteLine("Erro: " + err);
            }
        }
    }
}
