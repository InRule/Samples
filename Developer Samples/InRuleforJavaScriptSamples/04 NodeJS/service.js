var http = require('http');
var url = require('url');

//Lets define a port we want to listen to
const PORT=8080; 

var inrule = require('./Rating.min.js')

//We need a function which handles requests and send response
function handleRequest(request, response) {
    response.setHeader("Access-Control-Allow-Origin", "*");

    var parsedUrl = url.parse(request.url, true);
    var query = parsedUrl.query;
    
    var rate = {};
    rate.TermInYears = query.termInYears;

    var session = inrule.createRuleSession();
    session.createEntity('RateMgr', rate);
    session.applyRules(function(log){
       response.end(JSON.stringify(rate)); 
    });
}

//Create a server
var server = http.createServer(handleRequest);

//Lets start our server
server.listen(PORT, function(){
    //Callback triggered when server is successfully listening. Hurray!
    console.log("Server listening on: http://localhost:%s", PORT);
});