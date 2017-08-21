
function BuildUrl(textBox, relativeUrl) {
    textBox.value = document.getElementById('serviceUri').value + relativeUrl;
}

function SetServiceUrl(targetTextBox) {
    //this is the url that is pre-configured in the InRule.RuleServices project web.config
    var uri = "http://localhost:50001/InRule.RuleServices/RuleExecutionService.svc/";
    targetTextBox.value = uri;
}

function NavigateToServicesHelp() {
    var uri = document.getElementById('serviceUri').value;
    window.open(uri + "help");
}

function ExecuteRequest(workingDiv, button, verb, url, target, content) {

    try {
        target.innerText = "Processing Rule Execution Request...";

		var request = new XMLHttpRequest();
		request.open(verb, url, true);
		request.setRequestHeader("Cache-Control", "no-cache");
		request.setRequestHeader("Pragma", "no-cache");

		request.onreadystatechange = function () {
		    if (request.readyState == 4) {
		        var contentType = request.getResponseHeader("Content-type");
		        if (contentType.indexOf("text/xml") > -1) {
		            //process response type for ExecuteRuleRequest POST and Report formats
		            var renderAsHtml = false;
		            if (verb == "POST") {
		                var responseType = GetResponseTypeFromResponseXml(request.responseText);
		                if (responseType == "RuleExecutionReport" || responseType == "PerformanceStatisticsReport") {
		                    var report = DecodeXml(GetResponseTextFromResponseXml(request.responseText));
		                    target.innerHTML = report;
		                    renderAsHtml = true;
		                }
		            }
		            // display text responses 
		            if (renderAsHtml == false) {
		                target.innerText = request.responseText;
		            }
		        }
		        else {
		            //remove table style from rule service error html 
		            var responseText = CleanHtmlResponse(request);
		            target.innerHTML = responseText;
		        }
		        EnableControls(workingDiv, button, true);
		    }
		}

		EnableControls(workingDiv, button, false);

		if (verb == 'GET') {
			request.send();
		}
		else if (verb == 'POST') {
			request.setRequestHeader("Content-type", "application/xml");
			request.setRequestHeader("Content-length", content.length);
			request.setRequestHeader("Connection", "close");
			request.send(content);
		}
	}
	catch (err)
	{
		alert(err);
	}
}

var xml_to_escaped_map = {
  '&': '&amp;',
  '"': '&quot;',
  '\'': '&#039;',
  '<': '&lt;',
  '>': '&gt;'
};

var escaped_to_xml_map = {
  '&amp;': '&',
  '&quot;': '"',
  '&#039;': '\'',
  '&lt;': '<',
  '&gt;': '>'
};

function EncodeXml(string) {
  return string.replace(/([\&"'<>])/g, function(str, item) {
    return xml_to_escaped_map[item];
  });
};

function DecodeXml(string) {
  return string.replace(/(&amp;|&quot;|&#039;|&lt;|&gt;)/g,
  function(str, item) {
    return escaped_to_xml_map[item];
  });
};

function CleanHtmlResponse(request) {
    var lowerHtml = request.responseText.toLowerCase();
    if (lowerHtml.indexOf('<body') == -1) {
        if (request.responseText.length == 0) {
            return 'The request was either not accepted by the server, or no response was returned. Please verify URL of the service against cross-domain calls. ' + request.statusText;
        }
        // convert line breaks to br
        return request.responseText.replace(/\n/g, '<br/>');
    }
    else {
        var startIndex = lowerHtml.indexOf('<body') + 6;
        startIndex = lowerHtml.indexOf('>', startIndex) + 1;
        var endIndex = lowerHtml.indexOf('</body>');
        return request.responseText.slice(startIndex, endIndex);
    }
    
}

function EnableControls(workingDiv, button, enable) {
    button.disabled = !enable;
    if (enable) {
        workingDiv.style.display = 'none';
    }
    else {
        workingDiv.style.display = '';
    }
}

function ShowTab(form, li) {
    var divs = document.getElementsByTagName('div');
    for (var i = 0; i < divs.length; i++) {
        if (divs[i].id.substr(0, 4) == 'form') {
            if (divs[i].id == form) {
                divs[i].style.display = '';
            }
            else {
                divs[i].style.display = 'none';
            }
        }
    }

    var lis = document.getElementsByTagName('li');
    for (var i = 0; i < lis.length; i++) {
        if (lis[i].id == li) {
            lis[i].style.background = "#B0E0E6";
        }
        else
        {
            lis[i].style.background = "#eee";
        }
    }
}

function GetResponseTypeString(checkBoxContainer) {
    var checkBoxes = checkBoxContainer.getElementsByTagName('input');
    var output = "";
    for (var i = 0; i < checkBoxes.length; i++) {
        if (checkBoxes[i].checked) {
            if (output.length > 0) {
                output += "|";
            }
            output += checkBoxes[i].value;
        }
    }
    return output;
}

function GetResponseTextFromResponseXml(xml){
    var match = xml.match(/<ResponseText>([^<]*)<\/ResponseText>/);

    if (match == null)
        return "";
    else
        return match[1];
}

function GetResponseTypeFromResponseXml(xml){
    var match = xml.match(/<ResponseType>([^<]*)<\/ResponseType>/);

    if (match == null)
        return "";
    else
        return match[1];
}

function GetPostXml(targetInput, ruleApp, ruleSet, parameters, entity, entityXml, returnEntity, responseType, overrides) {
    
    var template = '<?xml version="1.0" encoding="utf-8" ?>\n'
         + '<RuleExecutionRequest xmlns="http://inrule.com/RuleServices">\n'
         + '<RuleApp>__ruleApp__</RuleApp>\n'
         + '<RuleSet>__ruleSet__</RuleSet>\n'
         + '<Parameters>__parameters__</Parameters>\n'
         + '<Entity>__entity__</Entity>\n'
         + '<EntityXml>__entityXml__</EntityXml>\n'
         + '<ReturnEntity>__returnEntity__</ReturnEntity>\n'
         + '<ResponseType>__responseType__</ResponseType>\n'
		 + '<EndPointOverrides>__endPointOverrides__</EndPointOverrides>\n'
         + '</RuleExecutionRequest>';

    template = template.replace('__ruleApp__', ruleApp);
    template = template.replace('__ruleSet__', ruleSet);
    template = template.replace('__parameters__', GetParametersXml(parameters));
    template = template.replace('__entity__', entity);
    template = template.replace('__entityXml__', XmlEncode(entityXml));
    template = template.replace('__returnEntity__', returnEntity);
    template = template.replace('__responseType__', responseType);
	template = template.replace('__endPointOverrides__', GetEndPointOverridesXml(overrides));
	
    targetInput.value = template;
}

function GetEndPointOverridesXml(grid)
{
	var returnValue = '';
	
	var memberTemplate = '<RuleExecutionEndPointOverride>\n'
		+ '<EndPointName>__endPointName__</EndPointName>\n'
		+ '<Properties>__properties__</Properties>\n'
		+ '</RuleExecutionEndPointOverride>\n';
		
	var propertyTemplate = '<RuleExecutionEndPointProperty>\n'
		+ '<Name>__name__</Name>\n'
		+ '<Value>__value__</Value>\n'
		+ '</RuleExecutionEndPointProperty>\n';
	
	for (var x = 0; x < grid.children.length; x++)
	{
		if (grid.children[x].nodeName == 'DIV')
		{
			var prefix = grid.children[x].id;
			if (prefix != '')
			{
			var name = document.getElementById(prefix + 'Name').value;
			
			if (name != '')
			{
				var properties = '';
				// locate child grid
				var propGrid = document.getElementById(prefix + 'ParamsContainer');
				var rows = propGrid.childNodes;
				for (var i = 0; i < rows.length; i++) {
					if (rows[i].id && rows[i].id != '') {
						var cells = rows[i].getElementsByTagName('div');
						if (cells[1].childNodes[0].value != '') {
							properties += propertyTemplate.replace('__name__', cells[1].childNodes[0].value).replace('__value__', XmlEncode(cells[2].childNodes[0].value));
						}
					}
				}
				
				returnValue += memberTemplate.replace('__endPointName__', name).replace('__properties__', properties);	
			}
			}
		}
	}
	
	return returnValue;
}

function GetParametersXml(grid) {

    var template = '<RuleExecutionParameter>\n'
        + '<Name>__name__</Name>'
        + '<Value>__value__</Value>'
        + '</RuleExecutionParameter>';

    var responseXml = '';
    var rows = grid.childNodes;
    for (var i = 0; i < rows.length; i++) {
        if (rows[i].id && rows[i].id != '') {
            var cells = rows[i].getElementsByTagName('div');
            if (cells[1].childNodes[0].value != '') {
                responseXml += template.replace('__name__', cells[1].childNodes[0].value).replace('__value__', XmlEncode(cells[2].childNodes[0].value));
            }
        }
    }

    return responseXml;
}

function GetParametersQueryString(grid) {
    
    var template = "&__name__=__value__";

    var responseString = '';
    var rows = grid.childNodes;
    for (var i = 0; i < rows.length; i++) {
        if (rows[i].id && rows[i].id != '') {
            var cells = rows[i].getElementsByTagName('div');
            if (cells[1].childNodes[0].value != '') {
                responseString += template.replace('__name__', cells[1].childNodes[0].value).replace('__value__', cells[2].childNodes[0].value);
            }
        }
    }

    return responseString;
}

function GetValueIfEnabled(control) {
    if (control.disabled) {
        return '';
    } else {
        return control.value;
    }
}

function XmlEncode(text) {
    text = text.replace(/&/g, '&amp;');
    text = text.replace(/\"/g, '&quot;');
    text = text.replace(/\'/g, '&apos;');
    text = text.replace(/</g, '&lt;');
    text = text.replace(/>/g, '&gt;');
    return text;
}

function DeleteParameter(grid, row) {
    grid.removeChild(row);
}

function AddParameter(grid, prefix) {
    // regex
    var endingDigits = new RegExp(/\d+$/);
    
    // select rows
    var rows = grid.getElementsByTagName('div')
    var lastId = 0;
    for (var i = 0; i < rows.length; i++) {
        var currentId = parseInt(endingDigits.exec(rows[i].id));
        if (currentId > lastId) {
            lastId = currentId;
        }
    }

    lastId += 1;

    var newRow = document.createElement('div');
    newRow.setAttribute('class', 'row');
    newRow.setAttribute('id', prefix + 'Row' + lastId);

    var newCell = document.createElement('div');
    newCell.setAttribute('class', 'firstcol');
    newImage = document.createElement('img');
    newImage.setAttribute('alt', 'delete');
    newImage.setAttribute('src', 'Content/Images/delete_12x12.png');
    newImage.setAttribute('onclick', 'DeleteParameter(' + prefix + 'ParamsContainer, ' + prefix + 'Row' + lastId + ');');
    newCell.appendChild(newImage);
    newRow.appendChild(newCell);

    var newCell2 = document.createElement('div');
    newCell2.setAttribute('class', 'col');

    var newInput = document.createElement('input');
    newInput.setAttribute('type', 'text');
    newInput.setAttribute('class', 'gridcell');
    newInput.setAttribute('id', prefix + 'Param' + lastId);
    newCell2.appendChild(newInput);

    newRow.appendChild(newCell2);

    var newCell3 = document.createElement('div');
    newCell3.setAttribute('class', 'col');

    var newInput2 = document.createElement('input');
    newInput2.setAttribute('type', 'text');
    newInput2.setAttribute('class', 'gridcell');
    newInput2.setAttribute('id', prefix + 'Value' + lastId);
    newCell3.appendChild(newInput2);

    newRow.appendChild(newCell3);

    grid.appendChild(newRow);

}

function AddEndPoint(grid, prefix)
{
	// regex
	var idPattern = new RegExp('^' + prefix + '\\d+');
    var endingDigits = new RegExp(/\d+$/);
    
    // select rows
    var rows = grid.getElementsByTagName('div')
    var lastId = 0;
    for (var i = 0; i < rows.length; i++) {
		if (idPattern.test(rows[i].id))
		{
			var currentId = parseInt(endingDigits.exec(rows[i].id));
			if (currentId > lastId) {
				lastId = currentId;
			}
		}
    }

    lastId += 1;
	var newPrefix = prefix + lastId;
	
	var rowTemplate = '<div class="firstcol" ><img alt="delete" src="Content/Images/delete_12x12.png" onclick="DeleteParameter(exreqEndPointContainer, __newPrefix__);" /></div>'
		+ '<div class="col"><span>Name:&nbsp;<input id="__newPrefix__Name" type="text" class="gridcell" style="border:1px solid #ddd;width:40%" /></span>'
		+ '<div id="__newPrefix__ParamsContainer" class="container" style="width:99%">'
		+ '<div class="row">'
		+ '<div class="headercol"></div>'
		+ '<div class="headercol">Property</div>'
		+ '<div class="headercol">Value</div>'
		+ '</div>'
		+ '<div class="row" id="__newPrefix__Row1" >'
		+ '<div class="firstcol" ><img alt="delete" src="Content/Images/delete_12x12.png" onclick="DeleteParameter(__newPrefix__ParamsContainer, __newPrefix__Row1);" /></div>'
		+ '<div class="col"><input id="__newPrefix__Param1" type="text" class="gridcell" /></div>'
		+ '<div class="col"><input id="__newPrefix__Value1" type="text" class="gridcell" /></div>'
		+ '</div>'
		+ '</div>'
		+ '<div>'
		+ '<input type="button" value="add property" class="button" onclick="AddParameter(__newPrefix__ParamsContainer, \'__newPrefix__\')" />'
		+ '</div>';
	rowTemplate = rowTemplate.replace(/__newPrefix__/g, newPrefix);
	
	var newRow = document.createElement('div');
	newRow.setAttribute('id', newPrefix);
	newRow.setAttribute('class', 'row');
	newRow.innerHTML = rowTemplate;
		
	grid.appendChild(newRow);
}


function EnableExplicitGetControls(enable) {
    document.getElementById('rsinputEntityNameLabel').disabled = !enable;
    document.getElementById('rsinputEntityName').disabled = !enable;
    document.getElementById('rsinputEntityXmlLabel').disabled = !enable;
    document.getElementById('rsinputEntityXml').disabled = !enable;

}

function EnableExplicitPostControls(enable) {
    document.getElementById('exreqEntityNameLabel').disabled = !enable;
    document.getElementById('exreqEntityName').disabled = !enable;
    document.getElementById('exreqEntityXmlLabel').disabled = !enable;
    document.getElementById('exreqEntityXml').disabled = !enable;
    document.getElementById('exreqRuleSetNameLabel').disabled = false;
    document.getElementById('exreqRuleSetName').disabled = false;
    document.getElementById('exreqParamsContainerLabel').disabled = false;
    document.getElementById('exreqParamsContainer').disabled = false;
    document.getElementById('exreqParamsButton').disabled = false;
}

function EnableAutoPostControls(enable) {
    document.getElementById('exreqRuleSetNameLabel').disabled = enable;
    document.getElementById('exreqRuleSetName').disabled = enable;
    document.getElementById('exreqParamsContainerLabel').disabled = enable;
    document.getElementById('exreqParamsContainer').disabled = enable;
    document.getElementById('exreqParamsButton').disabled = enable;
    document.getElementById('exreqEntityNameLabel').disabled = !enable;
    document.getElementById('exreqEntityName').disabled = !enable;
    document.getElementById('exreqEntityXmlLabel').disabled = !enable;
    document.getElementById('exreqEntityXml').disabled = !enable;

}


// functions to build test data
function buildGetTestData() {
    document.getElementById('inputRuleApp').value = 'MortgageCalculator';
    document.getElementById('inputEntityName').value = 'Mortgage';
    document.getElementById('inputReturnEntityName').value = 'Mortgage';
    document.getElementById('inputEntityXml').value = '<Mortgage><LoanInfo><PropertyId>1</PropertyId><Principal>300000</Principal><APR>7</APR><TermInYears>30</TermInYears></LoanInfo><PaymentSummary /></Mortgage>';
}

function buildPostTestData() {
    document.getElementById('exreqRuleApp').value = 'MortgageCalculator';
    document.getElementById('exreqEntityName').value = 'Mortgage';
    document.getElementById('exreqReturnEntityName').value = 'Mortgage';
    document.getElementById('exreqEntityXml').value = '<Mortgage><LoanInfo><PropertyId>1</PropertyId><Principal>300000</Principal><APR>7</APR><TermInYears>30</TermInYears></LoanInfo><PaymentSummary /></Mortgage>';
}

function buildGetRSTestData() {
    document.getElementById('rsinputRuleApp').value = 'MortgageCalculator';
    document.getElementById('rsinputRuleSet').value = 'CalcPaymentSchedule';
    document.getElementById('rsinputEntityName').value = 'Mortgage';
    document.getElementById('rsinputReturnEntityName').value = 'Mortgage';
    document.getElementById('rsinputEntityXml').value = '<Mortgage><LoanInfo><PropertyId>1</PropertyId><Principal>300000</Principal><APR>7</APR><TermInYears>30</TermInYears></LoanInfo><PaymentSummary /></Mortgage>';
}