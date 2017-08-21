var app = angular.module('mortgage', []);

app.controller('mortgageController', ['$scope', '$timeout', function ($scope, $timeout) {

    $scope.applyRules = function () {
        var session = inrule.createRuleSession();
        session.createEntity('Mortgage', $scope.mortgage);
        session.applyRules(function(log){});
    }

    $scope.calculatePaymentSchedule = function () {
        var session = inrule.createRuleSession();
        var mortgageEntity = session.createEntity('Mortgage', $scope.mortgage);
        // Need to run Auto-Sequental rules first, then our explicit ruleset
        session.applyRules(function(log){
            mortgageEntity.executeRuleSet("CalcPaymentSchedule", [], function(log){});    
        }); 
        
    }
}]);