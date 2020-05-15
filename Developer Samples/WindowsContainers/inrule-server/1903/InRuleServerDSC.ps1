 Configuration Website
 {
     param ($MachineName = "localhost")
     
   Node ($MachineName)
   {
     WindowsFeature WCFServices45
     {
         Name = "Net-WCF-Services45"
         Ensure = "Present"
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