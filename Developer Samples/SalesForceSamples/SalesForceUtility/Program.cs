using System;
using System.IO;
using System.Text;
using System.ServiceModel;
using SalesForceUtility.SFEnterpriseWSDL;
using InRule.Repository;
using InRule.Repository.RuleElements;
namespace InRule.SalesForce.Samples
{
    class ApiSample
    {
        private static SoapClient loginClient; // for login endpoint
        private static SoapClient client; // for API endpoint
        private static SessionHeader header;
        private static EndpointAddress endpoint;
        private LoginResult lr;
        private string username;
        private string password;
        private string ruleAppName;
        RuleApplicationDef ruleAppDef;
        DescribeGlobalResult sfObjectCache = null;
        
        static void Main(string[] args)
        {
            ApiSample sample = new ApiSample();
            sample.run();
        }
        public void run()
        {
            Console.WriteLine("\nWelcome to the SalesForce utility for InRule BRMS."); 
            if (login())
            {
                getRuleApp();
                ApiSample.help();
                commandLine();
            }
        }
        private void commandLine()
        {
            string line = "";
            cmd:
            Console.WriteLine("cmd>");
            Console.SetCursorPosition(Console.CursorLeft + 5, Console.CursorTop - 1);
            line = Console.ReadLine();
            //Simple Router
            string route = line;
            if (line.Length > 3) { route = line.Substring(0,4).Trim().ToLower(); }  
            switch (route)
            {
                case "help":
                    {
                        ApiSample.help();
                        goto cmd;
                    }
                case "info":
                    {
                        printUserInfo(lr, lr.serverUrl);
                        goto cmd;
                    }
                case "enti":
                    {
                        entity(line);
                        goto cmd;
                    }
                case "fiel":
                    {
                        field(line);
                        goto cmd;
                    }
                case "add":
                    {
                        add(line);
                        goto cmd;
                    }
                case "save": 
                    {
                        save();
                        break;
                    }
                case "quit":
                    {
                        break;
                    }
                default:
                    {
                        if (line.Trim() != "") { Console.WriteLine("oops..."); }
                        goto cmd;
                    }
            }
            logout();
        }
        private void getRuleApp() {
            Console.Write("\nType the name of of your RuleApp:  ");
            ruleAppName = Console.ReadLine();
            if (File.Exists(ruleAppName + ".ruleapp"))
            {
                ruleAppDef = InRule.Repository.RuleApplicationDef.Load(ruleAppName + ".ruleapp");
                Console.Write("\nThe ruleapp already exists. Actions will be merged into the existing file.");
            }
            else
            {
                ruleAppDef = new RuleApplicationDef(ruleAppName);
            }
        }
        private void save(){
            try {
                // Save the rule application to the file system.  Backup any existing file.
                if (File.Exists(ruleAppName + ".ruleapp"))
                {
                    File.Copy(ruleAppName + ".ruleapp", ruleAppName + "." + System.Guid.NewGuid().ToString() + ".ruleapp");
                    //InRule overwrites the file by default if it exists
                }
                ruleAppDef.SaveToFile(@ruleAppName + ".ruleapp");
                Console.WriteLine("Awesome! Your ruleapp named: " + ruleAppName + ".ruleapp may now be opened in irAuthor.");
            } catch (Exception e){
                Console.WriteLine("An exception has occurred: " + e.Message +
                    "\nStack trace: " + e.StackTrace);
            }
        }
        public static void help(){
            
            Console.ForegroundColor = ConsoleColor.White; Console.WriteLine("\nhelp ");  Console.ResetColor(); Console.WriteLine("- Present all the commands for this utility.");
            Console.ForegroundColor = ConsoleColor.White; Console.WriteLine("\ninfo "); Console.ResetColor(); Console.WriteLine("- Present the current session information.");
            Console.ForegroundColor = ConsoleColor.White; Console.WriteLine("\nentity [Entity Name Filter]"); Console.ResetColor(); Console.WriteLine("- List all of the entities available from SalesForce via the SOAP API.  Provide a few characters to filter." +
                "Note that objects may have a different names than presented from the site."); Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.White; Console.WriteLine("\nfield [Entity] [Field Name Filter]"); Console.ResetColor(); Console.WriteLine("- List all the fields for an entity.  Provide a few characters to filter");
            Console.ForegroundColor = ConsoleColor.White; Console.WriteLine("\nadd [entity].[field]"); Console.ResetColor(); Console.WriteLine("- Add a specific entity and corresponding field from SalesForce to a ruleapp.  " +
                "If the entity already exists then only the field is added.  No field specified loads the complete field list.");
            Console.ForegroundColor = ConsoleColor.White; Console.WriteLine("\nsave [filename]"); Console.ResetColor(); Console.WriteLine("- Save your work to a ruleapp.");
            Console.ForegroundColor = ConsoleColor.White; Console.WriteLine("\nquit"); Console.ResetColor(); Console.WriteLine("- Quit the utility."); 
            Console.WriteLine("\n");
        }
        public static DataType translateSalesForceType(string type)
        {
            if (type == "string") { return DataType.String; }
            if (type == "double") { return DataType.Number; }
            if (type == "date") { return DataType.Date; }
            if (type == "datetime") { return DataType.DateTime; }
            if (type == "boolean") { return DataType.Boolean; }
            if (type == "reference") { return DataType.String; }
            if (type == "textarea") { return DataType.String; }
            if (type == "picklist") { return DataType.String; }
            if (type == "email") { return DataType.String; }
            if (type == "id") { return DataType.String; }
            if (type == "complex") { return DataType.Complex; }  // TODO refector for deep models
            return DataType.String;
        }
        private bool login()
        {
            Console.Write("Enter username: ");
            username = Console.ReadLine();
            Console.Write("Enter password: ");
            password = Console.ReadLine();
            Console.Write("Enter SalesForce user key: ");
            password = password + Console.ReadLine();
            // Create a SoapClient specifically for logging in
            loginClient = new SoapClient();
            try
            {
                Console.WriteLine("\nLogging in...");
                lr = loginClient.login(null, username, password);
            }
            catch (Exception e)
            {
                Console.WriteLine("An unexpected error has occurred: " + e.Message);
                Console.WriteLine(e.StackTrace);
                return false;
            }
            // Check if the password has expired 
            if (lr.passwordExpired)
            {
                Console.WriteLine("An error has occurred. Your password has expired.");
                return false;
            }
            /** Once the client application has logged in successfully, it will use
             * the results of the login call to reset the endpoint of the service
             * to the virtual server instance that is servicing your organization
             */
            // On successful login, cache session info and API endpoint info
            endpoint = new EndpointAddress(lr.serverUrl);
            /** The sample client application now has a cached EndpointAddress
            * that is pointing to the correct endpoint. Next, the sample client
            * application sets a persistent SOAP header that contains the
            * valid sessionId for our login credentials. To do this, the sample
            * client application creates a new SessionHeader object. Add the session 
            * ID returned from the login to the session header
            */
            header = new SessionHeader();
            header.sessionId = lr.sessionId;
            // Create and cache an API endpoint client
            client = new SoapClient("Soap", endpoint);
            // Return true to indicate that we are logged in, pointed  
            // at the right URL and have our security token in place. 
            Console.SetCursorPosition(Console.CursorLeft + 13, Console.CursorTop - 1);
            Console.Write("OK");
            return true;
        }
        private void printUserInfo(LoginResult lr, String authEP)
        {
            try
            {
                GetUserInfoResult userInfo = lr.userInfo;
                Console.WriteLine("UserID: " + userInfo.userId);
                Console.WriteLine("User Full Name: " + userInfo.userFullName);
                Console.WriteLine("User Email: " + userInfo.userEmail);
                Console.WriteLine("SessionID: " + lr.sessionId);
                Console.WriteLine("Auth End Point: " + authEP);
                Console.WriteLine("Service End Point: " + lr.serverUrl);
                Console.WriteLine("RuleApp Name: " + ruleAppName);
                Console.WriteLine();
            }
            catch (Exception e)
            {
                Console.WriteLine("An unexpected error has occurred: " + e.Message +
                    " Stack trace: " + e.StackTrace);
            }
        }
        private void logout()
        {
            try
            {
                client.logout(header);
                Console.WriteLine("Logged out.");
                Console.ReadLine();
            }
            catch (Exception e)
            {
                Console.WriteLine("An unexpected error has occurred: " + e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }
        private void field(string line)
        {
            //parse the line
            string[] parts = line.Split(' ');
            string entity = ""; string filter = "";
            if (parts.Length > 1) { entity = parts[1]; }
            if (parts.Length > 2) { filter = parts[2]; }       
            Console.WriteLine("\nListing all fields for " + entity + "\n");
            if (parts.Length < 2)
            {
                Console.WriteLine("Oops...you are missing some things.");
            }
            else
            {
                try
                {
                    DescribeSObjectResult[] dsrArray;
                    client.describeSObjects(
                        header, // session header
                        null, // package version header
                        null, // locale options
                        new string[] { entity }, // passes the entity name into the array for the query
                        out dsrArray
                     );
                    // Since we described only one sObject, we should have only
                    // one element in the DescribeSObjectResult array.
                    DescribeSObjectResult dsr = dsrArray[0];
                    for (int x = 0; x < dsr.fields.Length; x++)
                    {
                        if ((dsr.fields[x].name.Contains(filter)) || (filter == ""))
                        {
                            Field field = dsr.fields[x];
                            Console.Write("\n" + field.name + "  -  " + translateSalesForceType(field.type.ToString()));
                            if (field.relationshipName != null) { Console.Write("  -  *" + field.relationshipName); }
                        }
                    }
                    Console.Write("\n");
                }
                catch (Exception e)
                {
                    Console.WriteLine("An unexpected error has occurred: " + e.Message);
                    Console.WriteLine(e.StackTrace);
                }
            }
        }
        private void entity(string line)
        {
            try
            {
                //parse the line
                string[] parts = line.Split(' ');
                string entity = ""; //string filter = "";
                if (parts.Length > 1) { entity = parts[1]; }
                //if (parts.Length > 2) { filter = parts[2]; }  
                if (sfObjectCache == null) //fill the cache
                {
                    //DescribeGlobalResult dgr;
                    client.describeGlobal(
                                header, // session header
                                null, // package version header
                                out sfObjectCache
                                );
                }
                Console.WriteLine("\nListing objects on SalesForce\n");
                {
                    // Loop through the array echoing the object names to the console             
                    for (int i = 0; i < sfObjectCache.sobjects.Length; i++)
                    {
                        if ((sfObjectCache.sobjects[i].name.Contains(entity)) || (entity == ""))
                        {
                            Console.WriteLine(sfObjectCache.sobjects[i].name);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("An exception has occurred: " + e.Message +
                    "\nStack trace: " + e.StackTrace);
            }
        }
        private bool proceed (string entity, string field) {
            bool resp = false;
            if ((entity != "") && (ruleAppDef.Entities.Contains(entity) == false) && (field == "")) { resp = true; }
            if ((entity != "") && (ruleAppDef.Entities.Contains(entity)) && (field != "")) { resp = true; }
            if ((entity != "") && (ruleAppDef.Entities.Contains(entity) == false) && (field != "")) { resp = true; }
            if ((entity != "") && (ruleAppDef.Entities.Contains(entity)) && (field == "")) {
                Console.WriteLine("oops...already have that one.");
            }
            return resp;
        }
        
        private void add (string line)
        {
            string[] parts = line.Split(' ');
            string entity = "";
            string eField = "";
            EntityDef entityDef = null; //working with a specific entitydef
            if (parts.Length > 1) { entity = parts[1]; }
            if (entity.Contains(".")) { 
                parts = entity.Split('.');
                entity = parts[0];
                eField = parts[1];
            }
            if (proceed(entity,eField)) //proceed with a little extra care
            {
                try
                {
                    
                    // Call describeSObjects() passing in an array with one object type name 
                    DescribeSObjectResult[] dsrArray;
                    client.describeSObjects(
                      header, // session header
                      null, // package version header
                      null, // locale options
                      new string[] { entity }, // object name array
                      out dsrArray
                      );
                    // one element in the DescribeSObjectResult array.
                    DescribeSObjectResult dsr = dsrArray[0];
                    if (ruleAppDef.Entities.Contains(entity) == false) //make new or use existing entitydef
                    {
                        entityDef = new EntityDef(entity);
                        ruleAppDef.Entities.Add(entityDef);
                    }
                    else
                    {
                        foreach (EntityDef ed in ruleAppDef.Entities)
                        {
                            if (ed.Name == entity)
                            {
                                entityDef = ed;
                            }
                        }
                    }
                    // Now, retrieve metadata for each field
                    for (int i = 0; i < dsr.fields.Length; i++)
                    {
                        if ((eField != "") && (eField != dsr.fields[i].name )) { continue; }
                        // Get the field 
                        Field field = dsr.fields[i];
                        FieldDef fd = new FieldDef();
                        fd.Name = field.name;
                        fd.DataType = translateSalesForceType(field.type.ToString());
                        fd.DisplayName = field.label;
                        fd.ValueList = null;
                        // Add Fields
                        
                        entityDef.Fields.Add(fd);
                        // Add entity fields
                        if (field.relationshipName != null)
                        {
                            //Does this Entity exist in the ruleapp?
                            if (ruleAppDef.Entities.Contains(field.relationshipName))
                            {
                                //confirming the entinty exists in the rule app
                                foreach (EntityDef ed in ruleAppDef.Entities)
                                {
                                    if (ed.Name == field.relationshipName.Trim())
                                    {
                                        FieldDef ef = new FieldDef();
                                        ef.Name = field.relationshipName;
                                        ef.DataType = DataType.Entity;
                                        ef.DataTypeEntityName = field.relationshipName;
                                        ef.DisplayName = field.relationshipName;
                                        //add the field to the current entityDef
                                        entityDef.Fields.Add(ef);
                                    }
                                }
                            }
                        }
                        //Translte types
                        Console.WriteLine("Add Field: " + field.name + "  Type: " +
                            translateSalesForceType(field.type.ToString()));
                        // TODO generate any helpers for the rule editor
                        // If this is a picklist field, show the picklist values   
                        //RuleApplicationDef.
                        /*
                        if (field.type.Equals(fieldType.picklist))
                        {
                            Console.WriteLine("\tPicklist Values");
                            for (int j = 0; j < field.picklistValues.Length; j++)
                            
                                Console.WriteLine("\t\t" + field.picklistValues[j].value);
                            } */
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("An exception has occurred: " + e.Message +
                        "\nStack trace: " + e.StackTrace);
                }
            }
        }

    }
}


