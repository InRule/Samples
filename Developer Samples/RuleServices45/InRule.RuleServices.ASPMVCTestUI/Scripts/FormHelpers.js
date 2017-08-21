function EnableForm(formName) {
    EnableControls(formName, true);
}

function DisableForm(formName) {
    EnableControls(formName, false);
}

function EnableControls(formId, enable) {
    var loadingDiv = document.getElementById('asyncWorking');
    if (enable) {
        loadingDiv.style.display = 'none';
    }
    else {
        loadingDiv.style.display = '';
    }

    var form = document.getElementById(formId);
    var allElements = form.elements;
    for (i = 0; i < allElements.length; i++) {
        allElements[i].disabled = !enable;
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

function AddParameter(grid, prefix) {
    // regex
    var endingDigits = new RegExp(/\d+$/);

    // select rows
    var rows = grid.getElementsByTagName('div');
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
    newImage.setAttribute('src', '../Content/delete_12x12.png');
    newImage.setAttribute('onclick', 'DeleteParameter(' + prefix + 'ParamsContainer, ' + prefix + 'Row' + lastId + ');');
    newCell.appendChild(newImage);
    newRow.appendChild(newCell);

    var newCell2 = document.createElement('div');
    newCell2.setAttribute('class', 'col');

    var newInput = document.createElement('input');
    newInput.setAttribute('type', 'text');
    newInput.setAttribute('class', 'gridcell');
    newInput.setAttribute('id', prefix + 'Param' + lastId);
    newInput.setAttribute('name', prefix + 'Param' + lastId);
    newCell2.appendChild(newInput);
    newRow.appendChild(newCell2);

    var newCell3 = document.createElement('div');
    newCell3.setAttribute('class', 'col');

    var newInput2 = document.createElement('input');
    newInput2.setAttribute('type', 'text');
    newInput2.setAttribute('class', 'gridcell');
    newInput2.setAttribute('id', prefix + 'Value' + lastId);
    newInput2.setAttribute('name', prefix + 'Value' + lastId);
    newCell3.appendChild(newInput2);

    newRow.appendChild(newCell3);

    grid.appendChild(newRow);

}

function DeleteParameter(grid, row) {
    grid.removeChild(row);
}


function EnableExplicitRuleSetControls(enable) {

    document.getElementById('RuleSetLabel').disabled = false;
    document.getElementById('RuleSet').disabled = false;
    document.getElementById('rsParamsContainerLabel').disabled = false;
    document.getElementById('rsParamsContainer').disabled = false;
    document.getElementById('rsParamsButton').disabled = false;
    document.getElementById('EntityLabel').disabled = !enable;
    document.getElementById('EntityXmlLabel').disabled = !enable;
    document.getElementById('Entity').disabled = !enable;
    document.getElementById('EntityXml').disabled = !enable;

}

function EnableAutoRuleSetControls(enable) {

    document.getElementById('RuleSetLabel').disabled = enable;
    document.getElementById('RuleSet').disabled = enable;
    document.getElementById('rsParamsContainerLabel').disabled = enable;
    document.getElementById('rsParamsContainer').disabled = enable;
    document.getElementById('rsParamsButton').disabled = enable;
    document.getElementById('EntityLabel').disabled = !enable;
    document.getElementById('EntityXmlLabel').disabled = !enable;
    document.getElementById('Entity').disabled = !enable;
    document.getElementById('EntityXml').disabled = !enable;

}


function AddEndPoint(grid, prefix) {
    // regex
    var idPattern = new RegExp('^' + prefix + '\\d+');
    var endingDigits = new RegExp(/\d+$/);

    // select rows
    var rows = grid.getElementsByTagName('div')
    var lastId = 0;
    for (var i = 0; i < rows.length; i++) {
        if (idPattern.test(rows[i].id)) {
            var currentId = parseInt(endingDigits.exec(rows[i].id));
            if (currentId > lastId) {
                lastId = currentId;
            }
        }
    }

    lastId += 1;
    var newPrefix = prefix + lastId;

    var rowTemplate = '<div class="firstcol" ><img alt="delete" src="../Content/delete_12x12.png" onclick="DeleteParameter(rsEndPointContainer, __newPrefix__);" /></div>'
		+ '<div class="col" style="padding:5px"><span>Name:&nbsp;<input id="__newPrefix__Name" name="__newPrefix__Name" type="text" class="gridcell" style="border:1px solid #ddd;width:40%" /></span>'
		+ '<div id="__newPrefix__ParamsContainer" class="container">'
		+ '<div class="row">'
		+ '<div class="headercol"></div>'
		+ '<div class="headercol">Property</div>'
		+ '<div class="headercol">Value</div>'
		+ '</div>'
		+ '<div class="row" id="__newPrefix__Row1" >'
		+ '<div class="firstcol" ><img alt="delete" src="../Content/delete_12x12.png" onclick="DeleteParameter(__newPrefix__ParamsContainer, __newPrefix__Row1);" /></div>'
		+ '<div class="col"><input id="__newPrefix__Param1" name="__newPrefix__Param1" type="text" class="gridcell" /></div>'
		+ '<div class="col"><input id="__newPrefix__Value1" name="__newPrefix__Value1" type="text" class="gridcell" /></div>'
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

// function to build sample data
function buildTestData(populateRuleSet) {
    document.getElementById('RuleApp').value = 'MortgageCalculator';
    document.getElementById('Entity').value = 'Mortgage';
    document.getElementById('ReturnEntity').value = 'Mortgage';
    if (populateRuleSet && document.getElementById('RuleSet')) {
        document.getElementById('RuleSet').value = 'CalcPaymentSchedule';
    }
    document.getElementById('EntityXml').value = '<Mortgage><LoanInfo><PropertyId>1</PropertyId><Principal>300000</Principal><APR>7</APR><TermInYears>30</TermInYears></LoanInfo><PaymentSummary /></Mortgage>';
    
}
