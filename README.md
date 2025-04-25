# ServerHighPerf

## Comment utiliser l'application

### Installation

- Cloner le repo
- Monter le conteneur Docker avec la commande "docker compose up --build"
- Accéder à l'application via l'URL http://localhost:5193

### Utilisation

- Possibilité de choisir le message à envoyer en remplissant le champ de texte
- Vous pouvez tester avec ce message :

  - L'envoi d'un unique message
  - L'envoi de ce message par X clients toutes les secondes
  - L'envoi de ce message X fois d'un coup par un client

## Tests

### Tests sur l'envoi d'un message en variant la taille de ce message

- Si l'on décide de tester sur un texte de 100 caractères (94 octets), une longueur assez typique d'un message de
  chat, on obtient :
  - une latence moyenne de 0.5 ms pour le WebSocket
  - une latence moyenne de 0.8 ms pour le gRPC

- Si l'on augmente la taille des messages au fur et à mesure :
  - 1000 caractères (949 octets) :
    - WebSocket : 0.5 ms
    - gRPC : 0.8 ms
  - 5000 caractères (2080 octets) :
    - WebSocket : 0.7 ms
    - gRPC : 0.8 ms
  - 10000 caractères (10295 octets) :
    - WebSocket : 0.8 ms
    - gRPC : 0.8 ms

- ==> Je ne peux pas monter plus au niveau du nombre de caractères, mais on voit une tendance qui s'en dégage.  
  Sur des messages courts plus usuels, le WebSocket va être plus adapté que le gRPC. Le WebSocket est moins
  structuré, avec moins de surcouche protocolaire. Il est donc plus léger et plus rapide pour des communications
  en temps réel et fréquentes comme un chat ou des notifications.

  Par contre, dès que la taille des messages augmente, le gRPC devient plus performant. Les performances du gRPC restent
  stables alors que celles du WebSocket augmentent. On peut penser que cette différence s'accentuerait encore plus avec des tailles
  de messages plus importantes. Le gRPC est donc plus adapté pour des échanges de données lourdes ou de payloads complexes.

  Avantage WebSocket sur les messages courts.  
  Avantage gRPC sur les messages longs.

### Tests sur l'envoi d'un message par X clients toutes les secondes

- On va tester dans un premier temps un message de 949 octets envoyé toutes les secondes par 100 clients :
  - WebSocket : 0.5 ms de latence médiane (plus représentative que la moyenne)
  - gRPC : 0.8 ms de latence médiane
  - => rien de probant, on reste sur les mêmes chiffres que précédemment.

- On va monter le nombre de clients :
  - 1000 clients :
    - WebSocket : il commence à montrer des signes de faiblesse, non pas au niveau de la latence, mais au niveau de la montée
      en charge. Les 1000 connexions ont du mal à se faire, c'est très ralenti sur la fin et on voit bien que l'arrivée des
      messages commence à se faire difficilement.
    - gRPC : pas de souci pour lui, la montée en charge se fait rapidement et la latence reste stable aux alentours de 2 ms.

  - 5000 clients :
    - WebSocket : la montée en charge est encore plus difficile et on observe une montée progressive de la latence dans le temps.
      En le laissant tourner un peu, elle monte à plus de 7000 ms.
    - gRPC : toujours pas de souci, montée en charge immédiate avec une latence stabilisée autour des 2 ms.

  - 10000 clients :
    - WebSocket : je ne vois plus rien, ça bloque tout simplement l'application !!
    - gRPC : toujours pas de souci, avec une latence qui monte aux alentours de 5 ms.

  - 100000 clients :
    - WebSocket : pas d'essai.
    - gRPC : ça fonctionne, mais la latence oscille beaucoup plus (entre 2 et plus de 1000 ms), on sent un ralentissement machine, mais ça tient le coup.

  - On pourrait refaire le test en changeant la taille du message, mais cela tournerait à l'avantage du gRPC qui verrait ses performances peu diminuer, tandis que côté
    WebSocket, les difficultés apparaîtraient plus rapidement.

- => On voit que le WebSocket va être bon pour des applications avec un nombre raisonnable de clients connectés (aux alentours de 1000 pour ma machine).
  On confirme bien les cas d'utilisation vus précédemment. Par contre, on met en lumière ici, dans ce test, l'utilisation du gRPC. Il va être plus adapté
  au streaming de données pour ainsi gérer des volumes massifs de clients et de messages avec stabilité. On passe à une échelle beaucoup plus importante.

### Tests sur l'envoi de X messages d'un coup par un seul client

- On reste sur un message de 949 octets envoyé 1000 fois :
  - WebSocket : 0.8 s
  - gRPC : 0.05 ms
- 5000 messages :
  - WebSocket : 3.3 s
  - gRPC : 0.2 ms
- 10000 messages :
  - WebSocket : 8 s
  - gRPC : 0.5 ms
- 50000 messages :
  - WebSocket : l'application sature et ralentit, les temps ne sont plus représentatifs
  - gRPC : 2 ms
- Si l'on continue pour le gRPC : 4.4 s pour 100000 messages, 6.3 s pour 200000 messages. Je n'ai pas testé plus haut car on sent quand même
  les limites de l'application.

- => Côté WebSocket, le temps nécessaire pour envoyer les messages croît de manière linéaire et on atteint rapidement les limites de l'application.
  Tandis que côté gRPC, la latence reste maîtrisée même pour des volumes de 200000 messages. Cela confirme la performance du gRPC pour l'envoi massif
  de messages et de données.

## Conclusion

### Comparaison des performances

- Le WebSocket va offrir d'excellentes performances sur de petits messages et des échanges fréquents. Il est plus léger et plus rapide. Mais il va saturer
  rapidement dès que la charge augmente.
- Le gRPC va être, lui, plus lent pour du petit contenu et de faible fréquence, mais il va rester très stable et plus performant pour des messages
  lourds et fréquents. Les tests montrent qu'il est capable d'encaisser des charges lourdes : 100000 clients ou 200000 messages sans problème.

### Origine technique

- Au niveau du format de données utilisé par les deux technologies : WebSocket va utiliser du JSON, un format texte non compressé, alors que le gRPC
  utilise du binaire. Le JSON est plus lourd et moins performant que le binaire.
- Le protocole sous-jacent : WebSocket utilise du TCP brut via HTTP/1.1, alors que le gRPC utilise du HTTP/2, plus performant, multiplexé et optimisé.
- Le gRPC permet d'utiliser un streaming actif bidirectionnel. Une fois la connexion ouverte, le client et le serveur peuvent s'échanger des messages
  dans les deux sens, en continu. Le WebSocket, lui, ne permet pas cela de manière native : il faut faire une requête pour chaque message.

### Cas d'usage

- **WebSocket** :
  - Application de chat : échange de messages courts et fréquents.
  - Notifications en live : un utilisateur reçoit des notifications en temps réel, en provenance d'autres utilisateurs.
  - Applications de collaboration en temps réel.
  - Dashboards en temps réel : affichage de données en temps réel, comme des graphiques ou des tableaux de bord, mais avec une quantité de données
    raisonnable.
  - Le WebSocket reste simple à mettre en place.

- **gRPC** :
  - Communication entre microservices : échange de données entre microservices, avec des messages lourds et fréquents. Les résultats sont rapides et formatés.
  - Appli mobile : comme l'envoi de données de localisation toutes les 10 secondes (rapide, léger et fiable).
  - Streaming de données : par exemple, l'envoi de fichiers de logs à un serveur. Le gRPC permet de compresser et d'optimiser le tout. L'envoi de
    données massives et très fréquentes, comme pour les flux de données boursières.
