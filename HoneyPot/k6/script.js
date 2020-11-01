import http from 'k6/http';
import { sleep } from 'k6';

export default function () {

    var url = 'http://host.docker.internal:4242/api/bees';
	
	http.post(
		url, 
		JSON.stringify({ name: 'Matilda', amount: Math.floor(Math.random() * 30) })
	);

	http.post(
		url, 
		JSON.stringify({ name: 'Lydia', amount: Math.floor(Math.random() * 30) })
	);

	//sleep(1);


	http.post(
		url, 
		JSON.stringify({name: 'robert', amount: Math.floor(Math.random() * 30) })
    );

	http.post(
		url,
		JSON.stringify({ name: 'hans', amount: Math.floor(Math.random() * 30) }));

    //sleep(1);
	
	
	http.post(
		url, 
		JSON.stringify({name: 'anne', amount: Math.floor(Math.random() * 30)})
	);

	http.post(
		url,
		JSON.stringify({ name: 'katarina', amount: Math.floor(Math.random() * 30) })
	);
}
