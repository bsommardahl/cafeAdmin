var pdf = require('html-pdf');
var http = require('http');
var moment = require('moment');
var request = require('request');	

function printDaily(start){
	var qstring = '?start=' + start + '&end=' + start + '&orderBy=product&action=Run+Report';

	request('http://localhost:3002/daily'+qstring, function (error, response, body) {
	  if (!error && response.statusCode == 200) {
	    pdf.create(response.body, { format: 'Letter' }).toFile(start+'.pdf', function(err, res) {
		  if (err) return console.log(err);
		  console.log(res);
		});
	  }
	  else{
	  	console.log(error);
	  }
	});	
}

for(var i = 0; i < 30; i++){
	var start = moment('2015-01-01').add(i, 'days').format('YYYY-MM-DD');
	printDaily(start);
}