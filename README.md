# honey_pot
Honey Pot :honey_pot: is a simple system that consists of an Kestrel server and a hosted service. Where Kestrel has a role of receiving POST request and store then into 
[non-blocking concurrent dictionary](https://github.com/VSadov/NonBlocking), while hosted service does the periodic polling on the dictionary. Hosted service on every poll interval is going to store the values to Redis and clear the dictionary.
