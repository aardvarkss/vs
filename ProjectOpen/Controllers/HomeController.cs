//using NHibernate;
using SDD.Models;
using SDD.Models.ViewModel;
using ProjectOpen.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ProjectOpen.Models;
using System.Diagnostics;
using System.Data;

namespace ProjectOpen.Controllers
{
    public class HomeController : Controller
    {
        List<string> sqls = new List<string>();
 
        /* Private helper method to display menus and breadcrumbs */

        private HeaderVM getHeaderInfo(List<string> bc, List<string> bcl, string title, string subTitle)
        {
            HeaderVM h = new HeaderVM();

            /* Page specifics - using parameters taken in */

            Breadcrumbs b = new Breadcrumbs();

            b.breadcrumbs = bc;

            b.breadcrumbsLink = bcl;

            h.brdcrm = b;

            HeadingLine hln = new HeadingLine();

            hln.GraphATitle = "Active Projects";
            hln.GraphBTitle = "Tasks Completed";

            List<int> l = new List<int> { 3, 7, 4, 6, 10, 12, 12, 12 };
            hln.GraphA = l;

            List<int> l2 = new List<int> { 16, 18, 13, 9, 12, 18, 19, 17, 3, 16, 12 };
            hln.GraphB = l2;

            hln.PageSubtitle = subTitle;
            hln.PageTitle = title;

            h.hl = hln;

            /* Generate sidebar menu */

            /* This probably needs to be database generated using the users access rights */

            SideBarMenu s = new SideBarMenu();
            MenuItem m = new MenuItem("icol-house", "/Home/Index", "Home");
            s.addMenuItem(m);

            MenuItem m8 = new MenuItem("icol-clipboard-text", "/Home/EDI", "EDI Rejections");
            s.addMenuItem(m8);

            MenuItem m2 = new MenuItem("icol-lock-unlock", "/Home/heldOrders", "Held Orders");
            s.addMenuItem(m2);

            MenuItem m7 = new MenuItem("icol-user", "/Home/PPO", "PPO Req Authorisation");
            s.addMenuItem(m7);//page-paste

            MenuItem m3 = new MenuItem("icol-alarm", "/Home/BackOrders", "Back Orders");
            s.addMenuItem(m3);

            MenuItem m4 = new MenuItem("icol-clipboard-text", "/Home/SOE", "Sales Order Enquiry");
            //s.addMenuItem(m4);

            MenuItem m9 = new MenuItem("icol-clipboard-text", "/Home/POE", "Purchase Order Enquiry");
            //s.addMenuItem(m9);

            MenuItem m5 = new MenuItem("icol-cog", "", "Administration");
            m5.addMenuSubItem(new MenuSubItem("icol-chart-organisation", "/Home/CSTeams", "CS Teams"));
            //m5.addMenuSubItem(new MenuSubItem("icol-award-star-gold", "", "Status"));
            //m5.addMenuSubItem(new MenuSubItem("icol-user-business-boss", "", "User"));
            s.addMenuItem(m5);

            MenuItem m6 = new MenuItem("icol-key", "", "Log Out");
            //s.addMenuItem(m6);

            h.sbm = s;

            HeaderBar hdr = new HeaderBar();

            h.Hdr = hdr;

            return h;
        }

        /* Home Page summarising all Transactional Processing Issues */

        [HttpGet]
        public ActionResult Index()
        {
            TeamSummary p = new TeamSummary();
            
            List<string> l = new List<string>();
            l.Add("Customer Experience");
            l.Add("");

            List<string> l2 = new List<string>();
            l2.Add("/Home/Index");
            l2.Add("/Home/Index");

            p.Header = getHeaderInfo(l, l2, "Customer Experience", "");

            return View(p);
        }

        /* Team Summary for each type of Issue */
        
        [HttpGet]
        public ActionResult PPO()
        {
            TeamSummary p = new TeamSummary();

            List<string> l = new List<string>();
            l.Add("Customer Experience");
            l.Add("PPO Awaiting Authorisation Team Summary");

            List<string> l2 = new List<string>();
            l2.Add("/Home/Index");
            l2.Add("/Home/PPO");

            p.Header = getHeaderInfo(l, l2, "PPO Awaiting Authorisation", "Team Summary");

            string s1 = "set isolation to dirty read;";
            string s2 = "set lock mode to wait;";

            string sql1 = "select  p.sales_orderno , p.sales_lineno , p.stock_no , p.suppno , s.supp_name, p.order_qty, p.unit_price, " +
                  "  p.order_qty * p.unit_price as value," +
                  "  p.order_date , p.due_date, p.authority_code, p.user_status5, h.custno ,c.team_name, c.teamid " +
                  "  from  mrppurprop p" +
                  "      inner join ordhead h" +
                  "          on p.compno = h.compno" +
                  "          and p.sales_orderno = h.orderno" +
                  "      inner join supplier s" +
                  "          on  p.compno = s.compno" +
                  "          and p.suppno = s.suppno" +
                  "      left outer join bbs_cs_team c" +
                  "          on c.teamid = get_team(h.compno , h.custno, h.wareno)" +
                  "  where p.index_fld3 = '0'" +
                  "  and   p.compno = 1" +
                  "  and   p.req_status = 4" +
                  "  and   p.sales_orderno != ''" +
                  "  into temp ppos with no log";


            string sql2 = "select team_name , teamid , count(distinct sales_orderno) as orders , count(*) as lines" +
                           "  from ppos" +
                           "   group by team_name, teamid";


            Database d = new Database();
            d.openConnection();

            d.executeSQL(s1);
            d.executeSQL(s2);

            d.executeSQL(sql1);
            var result1 = d.getDataset(sql2);
            

            d.closeConnection();

            p.Data = result1;

            return View(p);
        }

        [HttpGet]
        public ActionResult HeldOrders()
        {
            TeamSummary p = new TeamSummary();
            
            List<string> l = new List<string>();
            l.Add("Customer Experience");
            l.Add("Held Orders Team Summary");

            List<string> l2 = new List<string>();
            l2.Add("/Home/Index");
            l2.Add("/Home/HeldOrders");

            p.Header = getHeaderInfo(l, l2, "Held Orders", "");

            string s1 = "set isolation to dirty read;";
            string s2 = "set lock mode to wait;";

            string sql1 = "select h.custno, c.cust_name , h.orderno , l.line_no , l.stock_no , l.stock_desc, "+
                          "  l.order_qty , h.order_status as headerStatus , l.order_status as lineStatus, h.wareno, h.compno "+
                          "  from ordhead h inner join ordline l "+
                          "      on h.compno = l.compno "+
                          "      and h.orderno = l.orderno "+
                          "  inner join customer c "+
                          "      on h.compno = c.compno "+
                          "      and h.custno = c.custno "+
                          "  where h.compno = 1 "+
                          "  and h.wareno in (select wareno from waredesc) "+
                          "  and h.order_date > today - 2 "+
                          "  and l.order_status = 0 "+
                          "  into temp heldOrders with no log; ";
                   

            string sql2 =  "select team_name , teamid, count(distinct orderno) as orders,  count(*) as lines "+
                           "     from heldOrders h "+
                           "         left outer join bbs_cs_team cs "+
                           "             on cs.teamid = get_team(h.compno , h.custno, h.wareno) "+
                           "     group by team_name, teamid";


            string sql3 = "select * from heldOrders";

            Database d = new Database();
            d.openConnection();

            d.executeSQL(s1);
            d.executeSQL(s2);

            d.executeSQL(sql1);
            var result1 = d.getDataset(sql2);

            p.Data = result1;

            d.closeConnection();

            return View(p);
        }

        [HttpGet]
        public ActionResult BackOrders()
        {
            TeamSummary p = new TeamSummary();
            
            List<string> l = new List<string>();
            l.Add("Customer Experience");
            l.Add("Back Orders Team Summary");

            List<string> l2 = new List<string>();
            l2.Add("/Home/Index");
            l2.Add("/Home/BackOrders");

            p.Header = getHeaderInfo(l, l2, "Back Orders", "");


            string sql1 = "select h.compno , h.custno, h.order_ref, c.cust_name, l.create_date, l.order_date, l.due_date, l.orderno , "+
                          " l.line_no , l.stock_no , l.stock_desc, l.order_qty, l.desp_qty,"+
                          "      l.order_status, l.order_qty - l.desp_qty as outstanding_qty, l.non_stocked, l.back_orderno, l.back_lineno, l.wareno "+
                          "  from ordhead h"+
                          "      inner join ordline l on h.compno = l.compno and h.orderno = l.orderno"+
                          "      inner join customer c on h.compno = c.compno and h.custno = c.custno"+
                          "  where h.compno = 1"+
                          "  and h.wareno IN (select wareno from waredesc)"+
                          "  and h.order_date > today - 10"+
                          "  and h.order_status IN (1,2)"+
                          "  and h.due_date <= today"+
                          "  and l.due_date <= today"+
                          "  and l.order_status IN (1,2)"+
                          "  into temp backOrders with no log";

            string sql2 = " select b.* , promise_date, cs.team_name, teamid" +
                          "      from backOrders b"+
                          "          left outer join mrppurhed m on m.compno = 1 and b.back_orderno = m.order_no"+
                          "          left outer join mrppurdet d on m.compno = d.compno and m.order_no = d.order_no and b.back_lineno = d.line_no"+
                          "          left outer join bbs_cs_team cs"+
                          "              on cs.teamid = get_team(b.compno , b.custno, b.wareno)"+
                          "      into temp backOrders2 with no log";

            string sql3 = "select  team_name , teamid, count(distinct orderno) as orders, count(*) as lines" +
                           "     from backOrders2"+
                           "     group by team_name, teamid" +
                           "     order by team_name";


            string sql4 = "select * from backOrders2";

            Database d = new Database();
            d.openConnection();

            d.executeSQL(sql1);
            d.executeSQL(sql2);
            var result1 = d.getDataset(sql3);
            //var result2 = d.getDataset(sql4);

            p.Data = result1;

            d.closeConnection();

            return View(p);
        }

        [HttpGet]
        public ActionResult EDI()
        {
            TeamSummary p = new TeamSummary();

            List<string> l = new List<string>();
            l.Add("Customer Experience");
            l.Add("EDI Rejections Team Summary");

            List<string> l2 = new List<string>();
            l2.Add("/Home/Index");
            l2.Add("/Home/EDI");

            p.Header = getHeaderInfo(l, l2, "EDI Rejections", "");

            string s1 = "set isolation to dirty read;";
            string s2 = "set lock mode to wait;";

            string sql1 = "select genno , a.error_ind " +
                    "from swedierrlog a " +
                    "where message_type = 11 " +
                    "and genno > 10000000 " +
                    " and error_ind is not null " +
                    "and message_date = today " +
                    "and a.message not like 'WARNING%' " +
                    "and a.message not like '%Invoice%' " +
                    "into temp rejections_mn with no log; ";
                    
            string sql3 = "select  cs.team_name, cs.teamid ," +
                          "          count(genno) as rejections, " +
                          "          count(distinct c.doc_no) as lines " +
                          "  from rejections_mn a " +
                          "      inner join edisop_head b " +
                          "          on b.doc_no = a.genno " +
                          "      left outer join edisop_line c " +
                          "          on a.genno = c.doc_no " +
                          "          and a.error_ind = c.line_no " +
                          "      left outer join bbs_cs_team cs " +
                          "          on cs.teamid = get_team(b.compno , b.cust_acc_code, b.wareno) " +
                          "  group by team_name, teamid " +
                          "  ;";
            
            Database d = new Database();
            d.openConnection();

            d.executeSQL(s1);
            d.executeSQL(s2);

            d.executeSQL(sql1);
            
            var result1 = d.getDataset(sql3);
            

            p.Data = result1;

            d.closeConnection();

            return View(p);
        }

        /* List of Issues for the selected team */

        [HttpGet]
        public ActionResult PPOTeam(int Id)
        {
            TeamSummary p = new TeamSummary();
            Database d = new Database();
            d.openConnection();

            string sql69 = "select team_name from bbs_cs_team where teamid = " + Id + ";";
            var result11 = d.getDataset(sql69);

            string csTeam = result11.Tables[0].Rows[0]["team_name"].ToString();

            List<string> l = new List<string>();
            l.Add("Customer Experience");
            l.Add("PPO Awaiting Authorisation Team Summary");
            l.Add(csTeam);

            List<string> l2 = new List<string>();
            l2.Add("/Home/Index");
            l2.Add("/Home/PPO");
            l2.Add("/Home/PPOTeam/"+Id);

            p.Header = getHeaderInfo(l, l2, "PPO Awaiting Authorisation", csTeam);


            string sql1 = "select  p.sales_orderno , p.sales_lineno , p.stock_no , p.suppno , s.supp_name, p.order_qty, p.unit_price, " +
                  "  p.order_qty * p.unit_price as value," +
                  "  p.order_date , p.due_date, p.authority_code, p.user_status5, h.custno ,c.team_name, c.teamid " +
                  "  from  mrppurprop p" +
                  "      inner join ordhead h" +
                  "          on p.compno = h.compno" +
                  "          and p.sales_orderno = h.orderno" +
                  "      inner join supplier s" +
                  "          on  p.compno = s.compno" +
                  "          and p.suppno = s.suppno" +
                  "      left outer join bbs_cs_team c" +
                  "          on c.teamid = get_team(h.compno , h.custno, h.wareno)" +
                  "  where p.index_fld3 = '0'" +
                  "  and   p.compno = 1" +
                  "  and   p.req_status = 4" +
                  "  and   p.sales_orderno != ''" +
                  "  into temp ppos with no log";


            string sql3 = "select * from ppos where teamid = " + Id + ";";

            d.executeSQL(sql1);
            var result2 = d.getDataset(sql3);

            d.closeConnection();

            p.Data = result2;

            return View(p);
        }

        [HttpGet]
        public ActionResult HeldOrdersTeam(int Id)
        {
            TeamSummary p = new TeamSummary();

            Database d = new Database();
            d.openConnection();

            string sql69 = "select team_name from bbs_cs_team where teamid = " + Id + ";";
            var result11 = d.getDataset(sql69);

            string csTeam = result11.Tables[0].Rows[0]["team_name"].ToString();

            List<string> l = new List<string>();
            l.Add("Customer Experience");
            l.Add("Held Orders Team Sumary");
            l.Add(csTeam);

            List<string> l2 = new List<string>();
            l2.Add("/Home/Index");
            l2.Add("/Home/HeldOrders");
            l2.Add("/Home/HeldOrdersTeam/" + Id);

            p.Header = getHeaderInfo(l, l2, "Held Orders", "");

            string s1 = "set isolation to dirty read;";

            string sql1 = "select h.custno, c.cust_name , h.orderno , l.line_no , l.stock_no , l.stock_desc, " +
                          "  l.order_qty , h.order_status as headerStatus , l.order_status as lineStatus, h.wareno, h.compno " +
                          "  from ordhead h inner join ordline l " +
                          "      on h.compno = l.compno " +
                          "      and h.orderno = l.orderno " +
                          "  inner join customer c " +
                          "      on h.compno = c.compno " +
                          "      and h.custno = c.custno " +
                          "  where h.compno = 1 " +
                          "  and h.wareno in (select wareno from waredesc) " +
                          "  and h.order_date > today - 2 " +
                          "  and l.order_status = 0 " +
                          "  into temp heldOrders with no log; ";


            string sql3 =  "select * from heldOrders h "+
                           "       left outer join bbs_cs_team cs " +
                           "             on cs.teamid = get_team(h.compno , h.custno, h.wareno) " +
                           " where teamid = " + Id;

            

            d.executeSQL(s1);
            d.executeSQL(sql1);
            var result2 = d.getDataset(sql3);

            p.Data = result2;

            d.closeConnection();

            return View(p);
        }

        [HttpGet]
        public ActionResult BackOrdersTeam(int Id)
        {
            TeamSummary p = new TeamSummary();

            Database d = new Database();
            d.openConnection();

            string sql69 = "select team_name from bbs_cs_team where teamid = " + Id + ";";
            var result11 = d.getDataset(sql69);

            string csTeam = result11.Tables[0].Rows[0]["team_name"].ToString();

            List<string> l = new List<string>();
            l.Add("Customer Experience");
            l.Add("Back Orders Team Summary");
            l.Add(csTeam);

            List<string> l2 = new List<string>();
            l2.Add("/Home/Index");
            l2.Add("/Home/BackOrders");
            l2.Add("/Home/BackOrdersTeam/" + Id);

            p.Header = getHeaderInfo(l, l2, "Back Orders", csTeam);


            string sql1 = "select h.compno , h.custno, h.order_ref, c.cust_name, l.create_date, l.order_date, l.due_date, l.orderno , " +
                          " l.line_no , l.stock_no , l.stock_desc, l.order_qty, l.desp_qty," +
                          "      l.order_status, l.order_qty - l.desp_qty as outstanding_qty, l.non_stocked, l.back_orderno, l.back_lineno, l.wareno " +
                          "  from ordhead h" +
                          "      inner join ordline l on h.compno = l.compno and h.orderno = l.orderno" +
                          "      inner join customer c on h.compno = c.compno and h.custno = c.custno" +
                          "  where h.compno = 1" +
                          "  and h.wareno IN (select wareno from waredesc)" +
                          "  and h.order_date > today - 10" +
                          "  and h.order_status IN (1,2)" +
                          "  and h.due_date <= today" +
                          "  and l.due_date <= today" +
                          "  and h.next_pick_date <= today" +
                          "  and l.order_status IN (1,2)" +
                          "  into temp backOrders with no log";

            string sql2 = " select b.* , promise_date, cs.team_name, teamid" +
                          "      from backOrders b" +
                          "          left outer join mrppurhed m on m.compno = 1 and b.back_orderno = m.order_no" +
                          "          left outer join mrppurdet d on m.compno = d.compno and m.order_no = d.order_no and b.back_lineno = d.line_no" +
                          "          left outer join bbs_cs_team cs" +
                          "              on cs.teamid = get_team(b.compno , b.custno, b.wareno)" +
                          "      into temp backOrders2 with no log";


            string sql4 = "select * from backOrders2 where teamid = " + Id + " order by orderno , line_no; ";

            d.executeSQL(sql1);
            d.executeSQL(sql2);
       
            var result2 = d.getDataset(sql4);

            p.Data = result2;

            d.closeConnection();

            return View(p);
        }

        [HttpGet]
        public ActionResult EDITeam(int Id)
        {
            TeamSummary p = new TeamSummary();

            Database d = new Database();
            d.openConnection();

            string sql69 = "select team_name from bbs_cs_team where teamid = " + Id + ";";
            var result11 = d.getDataset(sql69);

            string csTeam = result11.Tables[0].Rows[0]["team_name"].ToString();

            List<string> l = new List<string>();
            l.Add("Customer Experience");
            l.Add("EDI Rejections Team Summary");
            l.Add(csTeam);

            List<string> l2 = new List<string>();
            l2.Add("/Home/Index");
            l2.Add("/Home/EDI");
            l2.Add("/Home/EDITeam/" + Id);

            p.Header = getHeaderInfo(l, l2, "EDI Rejections", csTeam);

            string s1 = "set isolation to dirty read;";
            string s2 = "set lock mode to wait;";

            string sql1 = "select genno , a.error_ind, a.supplier, a.reference, a.message, a.lineno, a.date_created, a.time_created " +
                    "from swedierrlog a " +
                    "where message_type = 11 " +
                    "and genno > 15000000 " +
                    " and error_ind is not null " +
                    "and message_date = today " +
                    "and a.message not like 'WARNING%' " +
                    "and a.message not like '%Invoice%' " +
                    "into temp rejections_mn with no log; ";

            string sql3 = "select  cs.team_name, cs.teamid ," +
                          "          a.genno, " +
                          "          a.error_ind,  " +
                          " a.supplier,a.reference,a.message,b.contract_no,b.delivery_name_1,b.delivery_name_2,b.delivery_addr_1,b.delivery_addr_2,b.delivery_addr_3,b.delivery_addr_4, "+
                          " b.delivery_post_code,b.orderno,b.wareno,c.line_number,c.hmso_product_code,c.qty_ordered,c.price,c.item_desc_1,c.item_desc_2,c.item_desc_3,c.item_desc_4, "+
                          " c.item_desc_5,cs.team_name,a.lineno,c.line_no , a.date_created, a.time_created" +        
                          "  from rejections_mn a " +
                          "      inner join edisop_head b " +
                          "          on b.doc_no = a.genno " +
                          "      left outer join edisop_line c " +
                          "          on a.genno = c.doc_no " +
                          "          and a.error_ind = c.line_no " +
                          "      left outer join bbs_cs_team cs " +
                          "          on cs.teamid = get_team(b.compno , b.cust_acc_code, b.wareno) " +
                          "  where cs.teamid = " + Id +
                          " order by a.date_created, a.time_created,a.supplier,a.reference ;";

            d.executeSQL(s1);
            d.executeSQL(s2);

            bool res = d.executeSQL(sql1);

            DataSet result1 = new DataSet(); ;

            if(res)
            {
                result1 = d.getDataset(sql3);
            }

            p.Data = result1;

            d.closeConnection();

            return View(p);
        }


    }
}