// This loads the environment variables from the .env file
var env = require('dotenv-extended').load();

var builder = require('botbuilder');
var restify = require('restify');
var Store = require('./store');

// Setup Restify Server
var server = restify.createServer();
server.listen(env.port || env.PORT || 3978, function () {
    console.log('%s listening to %s', server.name, server.url);
});

// Create chat bot
var connector = new builder.ChatConnector({
    appId: env.MICROSOFT_APP_ID,
    appPassword: env.MICROSOFT_APP_PASSWORD
});
var bot = new builder.UniversalBot(connector);
server.post('/api/messages', connector.listen());

// You can provide your own model by specifing the 'LUIS_MODEL_URL' environment variable
// This Url can be obtained by uploading or creating your model from the LUIS portal: https://www.luis.ai/
const LuisModelUrl = env.LUIS_MODEL_URL ||
    'https://api.projectoxford.ai/luis/v1/application?id=162bf6ee-379b-4ce4-a519-5f5af90086b5&subscription-key=11be6373fca44ded80fbe2afa8597c18';

// Main dialog with LUIS
var recognizer = new builder.LuisRecognizer(LuisModelUrl);
var intents = new builder.IntentDialog({ recognizers: [recognizer] })
    .matches('查询宾馆', [
        function (session, args, next) {
            session.send('您好，我是宾馆搜索机器人! 我们正在分析您的消息\'%s\'，请耐心等候。。。', session.message.text);

            // try extracting entities
            var cityEntity = builder.EntityRecognizer.findEntity(args.entities, '地点');
            if (cityEntity) {
                // city entity detected, continue to next step
                session.dialogData.searchType = '地点';
                next({ response: cityEntity.entity });
            } else {
                // no entities detected, ask user for a destination
                builder.Prompts.text(session, '请输入您的目的地：');
            }
        },
        function (session, results) {
            var destination = results.response;

            var message = '正在寻找宾馆';
            if (session.dialogData.searchType === 'airport') {
                message += ' near %s airport...';
            } else {
                message += ' in %s...';
            }

            session.send(message, destination);

            // Async search
            Store
                .searchHotels(destination)
                .then((hotels) => {
                    // args
                    session.send('我找到%d宾馆:', hotels.length);

                    var message = new builder.Message()
                        .attachmentLayout(builder.AttachmentLayout.carousel)
                        .attachments(hotels.map(hotelAsAttachment));

                    session.send(message);

                    // End
                    session.endDialog();
                });
        }
    ])
    .matches('帮助', builder.DialogAction.send('Hi! Try asking me things like \'search hotels in Seattle\', \'search hotels near LAX airport\' or \'show me the reviews of The Bot Resort\''))
    .matches('None', builder.DialogAction.send('Hi! Try aski'))
    .onDefault((session) => {
        session.send('对不起，我们只提供宾馆查找服务！', session.message.text);
    });

bot.dialog('/', intents);

// Helpers
function hotelAsAttachment(hotel) {
    return new builder.HeroCard()
        .title(hotel.name)
        .subtitle('%d stars. %d reviews. From $%d per night.', hotel.rating, hotel.numberOfReviews, hotel.priceStarting)
        .images([new builder.CardImage().url(hotel.image)])
        .buttons([
            new builder.CardAction()
                .title('More details')
                .type('openUrl')
                .value('https://www.bing.com/search?q=hotels+in+' + encodeURIComponent(hotel.location))
        ]);
}

function reviewAsAttachment(review) {
    return new builder.ThumbnailCard()
        .title(review.title)
        .text(review.text)
        .images([new builder.CardImage().url(review.image)])
}