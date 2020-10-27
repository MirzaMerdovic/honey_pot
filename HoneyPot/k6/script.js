import http from 'k6/http';
import { sleep } from 'k6';

export default function () {
    http.get('http://host.docker.internal:4242/api/bees?id=denmark');
  sleep(1);
}
