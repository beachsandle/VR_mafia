const app = require('express')();
const server = require('http').createServer(app);
const io = require('socket.io')(server);


app.get('/', (req, res)=>{
  res.sendFile(__dirname + '/index.html');
});
 
server.listen(3000, ()=>{
  console.log('Socket IO server listening on port 3000');
});