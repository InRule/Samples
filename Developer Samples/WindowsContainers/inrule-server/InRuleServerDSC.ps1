 Configuration Website
 {
     param ($MachineName = "localhost")
     
   Node ($MachineName)
   {
     WindowsFeature WebServerRole
     {
       Name = "Web-Server"
       Ensure = "Present"
     }

     WindowsFeature WebAspNet45
     {
       Name = "Web-Asp-Net45"
       Ensure = "Present"
       DependsOn = "[WindowsFeature]WebServerRole"
     }

     WindowsFeature WCFServices45
     {
         Name = "Net-WCF-Services45"
         Ensure = "Present"
         DependsOn = "[WindowsFeature]WebAspNet45"
     }

     WindowsFeature WCFHttpActivation
     {
         Name = "Net-WCF-Http-Activation45"
         Ensure = "Present"
         DependsOn = "[WindowsFeature]WCFServices45"
     }    
      WindowsFeature WCFTCPPortSharing
     {
         Name = "Net-WCF-TCP-PortSharing45"
         Ensure = "Present"
         DependsOn = "[WindowsFeature]WCFHttpActivation"
     }      
   }
 }

 Website -MachineName localhost
 Start-DscConfiguration -Path .\Website -Wait -Verbose