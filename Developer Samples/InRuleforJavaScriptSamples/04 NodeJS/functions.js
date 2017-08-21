function getAprByTerm(termInYears, callback) {

    var uri = "http://localhost:8080/?termInYears=" + termInYears;

    var rate = 0;

    $.get(uri)
         .success(function (result) {
             rate = JSON.parse(result).Rate;
             callback(null, rate);
         })
         .error(function (jqXHR, textStatus, errorThrown) {
             callback(null, 0);
         });
}