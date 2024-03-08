
const dhive = require('@hiveio/dhive');
const client = new dhive.Client('https://api.hive.blog');

const bodyParser = require('body-parser');
const cors = require('cors');
const express = require('express');
require('dotenv').config();

const router = express.Router();
const port = 3000;
const app = express();
app.use(bodyParser.json());
app.use(
    bodyParser.urlencoded({
      extended: false,
    }),
  );
  app.use(cors()); 

router.get('/', async (req, res) => {
  console.log("Got a GET");
    res.send("Hello");
})

router.post('/', async (req, res) => {
    try{
        const username = 'user1427';
        const privateKey = dhive.PrivateKey.fromString(
            process.env.PRIVATE_KEY
        );
        const type = 'HIVE';
        const recipient= req.body.recipient;
        const comments = req.body.comments;
        const quantity = Math.round(parseFloat(req.body.quantity)*1000)/1000;
        const amt = quantity + ' ' + type;

        //create transfer object
        const transf = new Object();
        transf.from = username;
        transf.to = recipient;
        transf.amount = amt;
        transf.memo = comments;

        console.log(recipient);
        console.log(quantity);

        // res.send("Data processed");
        client.broadcast.transfer(transf, privateKey).then(
            function(result) {
              console.log(result);
                console.log(
                    'included in block: ' + result.block_num,
                    'expired: ' + result.expired
                );
                res.status(200).json({ message: "Success!" });
            },
            function(error) {
                console.error(error);
                res.status(400).json({ message: "Failure!" });
            }   
    );
    } catch (e) {
            return res.json({
              id: 0,
              error: 'Unexpected error.'
            })
          }

    })

    app.use("/", router);
    app.listen(port, () => {
        console.log(`Example app listening on port ${port}`)
      })
