// Load required modules.
const AWS = require('aws-sdk');
const Alexa = require('alexa-sdk');
const sqsQueueUrl = process.env.SQS_QUEUE_URL;
const appId = process.env.APP_ID;

// TODO: make this look at dynambo instead of json sons

var SONGS = {
    "100": "Carry On",
    "101": "Alone",
    "102": "Who Knew",
    "103": "Someone Like You",
    "104": "Over You",
    "105": "Stay",
    "106": "When I Was Your Man",
    "107": "Teenage Dream",
    "108": "Looking For a New Love",
    "109": "What Doesn't Kill You (Stronger)",
    "110": "After All These Years",
    "111": "Keep Your Head Up",
    "112": "Girl On Fire",
    "113": "I Love You",
    "114": "Said I Loved You..But I Lied",
    "115": "Express Yourself",
    "116": "I Want to Know What Love Is",
    "117": "Angel Eyes",
    "118": "Don't Mean Nothing",
    "119": "Hello",
    "120": "Get Lucky",
    "121": "Home",
    "122": "Valentine",
    "123": "All The Right Moves",
    "124": "Wish You Were Here",
    "125": "Reach",
    "126": "Fight Song",
    "127": "Little Lies",
    "128": "Uptown Funk [feat Bruno Mars]",
    "129": "Better in Time",
    "130": "Time After Time",
    "131": "You And I",
    "132": "Always",
    "133": "That's What Friends Are For",
    "134": "Danger Zone",
    "135": "Almost Paradise",
    "136": "Take My Breath Away",
    "137": "I Wish It Would Rain Down",
    "138": "Let's Hear It For The Boys",
    "139": "End of the Innocence",
    "140": "Greatest Love of All",
    "141": "Lady",
    "142": "Imagine",
    "143": "The Sign",
    "144": "I'll Always Love You",
    "145": "All By Myself",
    "146": "Moves Like Jagger",
    "147": "Wanted Dead or Alive",
    "148": "This Kiss",
    "149": "Tonight's The Night",
    "150": "Love Story",
    "151": "Because You Loved Me",
    "152": "Every Rose Has It's Thorn",
    "153": "A Hazy Shade of Winter",
    "154": "If You Leave Me Now",
    "155": "Shower Me With Your Love",
    "156": "(Everything I Do) I Do It For You",
    "157": "Don't Worry Be Happy",
    "158": "The Finer Things",
    "159": "Three Times A Lady",
    "160": "Songbird",
    "161": "Handle Me With Care",
    "162": "Just As I Am",
    "163": "Here and Now",
    "164": "What a Wonderful World",
    "165": "Rock On",
    "166": "My Kinda Lover",
    "167": "Glory of Love",
    "168": "Here I Go Again",
    "169": "Could've Been",
    "170": "Crazy",
    "171": "Every Breath You Take",
    "172": "Flashdance..What a Feeling",
    "173": "Mirrors",
    "174": "Just a Kiss",
    "175": "Stairway To Heaven",
    "176": "Open Arms",
    "177": "Self Control",
    "178": "Home",
    "179": "Missing You",
    "200": "We Are Young",
    "201": "These Dreams",
    "202": "Blow Me (One Last Kiss)",
    "203": "Set Fire To The Rain",
    "204": "No Surprise",
    "205": "We Found Love",
    "206": "Treasure",
    "207": "Roar",
    "208": "Real Love",
    "209": "Because of You",
    "210": "Amazed",
    "211": "Just Like Heaven",
    "212": "Hurt",
    "213": "Blurred Lines",
    "214": "Chariots of Fire",
    "215": "Open Your Heart",
    "216": "I Have Waited For So Long",
    "217": "You Were Meant for Me",
    "218": "The Flame of Love",
    "219": "Remedy",
    "220": "Iris",
    "221": "Feels Like Tonight",
    "222": "Let It Go",
    "223": "Counting Stars",
    "224": "Any Colour You Like",
    "225": "Don't Want to Lose You Now",
    "226": "Like I'm Going to Lose You",
    "227": "You Make Loving Fun",
    "228": "Thinking Out Load",
    "229": "Bleeding Love",
    "230": "True Colors",
    "231": "You Can't Run From Love",
    "232": "All In The Name of Love",
    "233": "I'll Never Love This Way Again",
    "234": "Celebrate Me Home",
    "235": "Here Comes the Rain Again",
    "236": "Torn",
    "237": "Another Day in Paradise",
    "238": "Hey Delilah",
    "239": "If Dirt Were Dollars",
    "240": "Thinking About You",
    "241": "Babe",
    "242": "Woman",
    "243": "Don't Turn Around",
    "244": "Don't Rush Me",
    "245": "Make Me Lose Control",
    "246": "Daylight",
    "247": "You Give Love a Bad Name",
    "248": "Breathe",
    "249": "The First Cut Is The Deepest",
    "250": "I Knew You Were Trouble",
    "251": "Let's Talk About Love",
    "252": "Fallen Angel",
    "253": "Manic Monday",
    "254": "Colour My World",
    "255": "Cups",
    "256": "Straight From The Heart",
    "257": "Simple Pleasure",
    "258": "Say Something",
    "259": "Too Hot Ta Trot",
    "260": "Midnight Motion",
    "261": "Margarita",
    "262": "Crazy Love",
    "263": "Dance with My Father",
    "264": "La Bamba",
    "265": "Gone, Gone, Gone",
    "266": "Everybody Wants You",
    "267": "No Explanation",
    "268": "Is This Love",
    "269": "True",
    "270": "Your Cheating Heart",
    "271": "Murder by Numbers",
    "272": "Love Theme From Flashdance",
    "273": "Suit & Tie",
    "274": "Need You Now",
    "275": "All My Love",
    "276": "Faithfully",
    "277": "Gloria",
    "278": "Haven't Met You Yet",
    "279": "Picture",
    "300": "My Own Worst Enemy",
    "301": "Burn",
    "302": "Sweater Weather",
    "303": "I'm Like a Bird",
    "304": "Daughters",
    "305": "I'll Make Love to You",
    "306": "Some Nights",
    "307": "If You Don't Know Me By Now",
    "308": "You Raise Me Up",
    "309": "Leather and Lace",
    "310": "I Just Died In Your Arms Tonight",
    "311": "Jar of Hearts",
    "312": "Raise Your Glass",
    "313": "Wherever You Will Go",
    "314": "You and Me",
    "315": "Come Away With Me",
    "316": "I Love You",
    "317": "I Love You Always Forever",
    "318": "More Than Words",
    "319": "Animals",
    "320": "Safe and Sound",
    "321": "I Melt With You",
    "322": "Far Away",
    "323": "Best Day of My Life",
    "324": "I'm Yours",
    "325": "Can't Help Falling in Love",
    "326": "Locked Out of Heaven",
    "327": "Bubbly",
    "328": "I Will Wait",
    "329": "Story of My Life",
    "330": "Down Under",
    "331": "I'll Be",
    "332": "The Way I Am",
    "333": "Payphone",
    "334": "God Blessed the Broken Road",
    "335": "What Makes You Beautiful",
    "336": "DJ Got Us Fallin' in Love",
    "337": "Call Me Maybe",
    "338": "Happy",
    "339": "Feel This Moment",
    "340": "Landslide",
    "341": "Can't Fight This Feeling",
    "342": "Photograph",
    "343": "Black Velvet",
    "344": "With or Without You",
    "345": "The Reason",
    "346": "Wake Me Up",
    "347": "All of Me",
    "348": "I'll Stand by You",
    "349": "Cold Hearted",
    "350": "How Will I Know",
    "351": "Love Somebody",
    "352": "Demons",
    "353": "What's Up",
    "354": "Hello",
    "355": "Brave",
    "356": "Ordinary World",
    "357": "Strong Enough",
    "358": "Keep On Loving You",
    "359": "In Your Eyes",
    "360": "You're Beautiful",
    "361": "Back To December",
    "362": "Jessie's Girl",
    "363": "Oh Sheila",
    "364": "It Must Have Been Love",
    "365": "The Middle",
    "366": "Don't Dream It's Over",
    "367": "What Hav I Done To Deserve This",
    "368": "Done You (Forget About Me)",
    "369": "Poker Face",
    "370": "Human Nature",
    "371": "What Hurts the Most",
    "372": "The Way You Make Me Feel",
    "373": "Ho Hey",
    "374": "Borderline",
    "375": "Pride (In the Name of Love)",
    "376": "Before He Cheats",
    "377": "I'm Already There",
    "378": "Bad Romance",
    "379": "A Little Respect",
    "380": "Shout",
    "381": "This Love",
    "382": "Centerfield",
    "383": "I Can't Make You Love Me",
    "384": "Sister Christian",
    "385": "Hold Me Now",
    "386": "Rio",
    "387": "You Keep Me Hangin' On",
    "388": "I Just Called to Say I Love You",
    "389": "I Hope You Dance",
    "390": "Just Give Me a Reason",
    "391": "Dark Horse",
    "392": "All Summer Long",
    "393": "Let Her Go",
    "394": "A Moment Like This",
    "395": "Friday I'm In Love",
    "396": "Enjoy the Silence",
    "397": "Mr. Jones",
    "398": "A Thousand Years",
    "399": "Irreplacable",
    "400": "I Can't Go For That (No Can Do)",
    "401": "You Make Me Feel Like Dancing",
    "402": "Nights In White Satin",
    "403": "Fooled Around And Feel In Love",
    "404": "Baby Come To Me",
    "405": "Sara Smile",
    "406": "One More Try",
    "407": "Danny's Song",
    "408": "Reminiscing",
    "409": "Saving All My Love For You",
    "410": "That's What Love Is For",
    "411": "Too Much Heaven",
    "412": "Overjoyed",
    "413": "Gypsy",
    "414": "Baby Come Back",
    "415": "Love Will Lead You Back",
    "416": "Love Will Keep Us Alive",
    "417": "Never Be The Same",
    "418": "You Wear It Well",
    "419": "Listen To Your Heart",
    "420": "Just You 'N' Me",
    "421": "Looks Like We Made It",
    "422": "There'll Be Sad Songs (To Make You Cry)",
    "423": "Kokomo",
    "424": "Kiss On My List",
    "425": "Goodbye Yellow Brick Road",
    "426": "Too Late To Turn Back Now",
    "427": "Here I Am",
    "428": "Breakaway",
    "429": "Right Here Waiting",
    "430": "Feel Like Makin' Love",
    "431": "We've Only Just Begun",
    "432": "You're So Vain",
    "433": "Rhiannon",
    "434": "Up Where We Belong",
    "435": "The Lady in Red",
    "436": "September Morn",
    "437": "Stand By Me",
    "438": "Dreams",
    "439": "Escape (The Pina Colada Song)",
    "440": "If You Don't Know Me By Now",
    "441": "You're In My Heart",
    "442": "The Long Run",
    "443": "I Will Remember You",
    "444": "One More Night",
    "445": "Steal Away",
    "446": "Me & Mrs. Jones",
    "447": "Sara",
    "448": "The Power of Love",
    "449": "Ride Like The Wind",
    "450": "Hold On To The Nights",
    "451": "One On One",
    "452": "Your Song",
    "453": "I Just Want to be Your Everything",
    "454": "Brown Eyed Girl",
    "455": "I'd Really Love To See You Tonight",
    "456": "Still The Same",
    "457": "Wonderful Tonight",
    "458": "Rock with You",
    "459": "Sharing The Night Together",
    "460": "Maggie May",
    "461": "Anticipation",
    "462": "All Out of Love",
    "463": "Neither One of Us (Wants To Be The First to Say Goodbye)",
    "464": "Lights",
    "465": "Sweet Caroline",
    "466": "Something",
    "467": "Daniel",
    "468": "Hold Me",
    "469": "Sad Eyes",
    "470": "Can't We Try",
    "471": "And I love Her",
    "472": "Time in a Bottle",
    "473": "All By Myself",
    "474": "Cherish",
    "475": "Fire and Rain",
    "476": "Baby What a Big Surprise",
    "477": "(I've Had) The Time Of My Life",
    "478": "Don't Let the Sun Go Down On Me",
    "479": "Red Red Wine",
    "480": "Sometimes When We Touch",
    "481": "I'll Have To Say I Love You In A Song",
    "482": "Get Closer",
    "483": "What's Love Got to Do With It",
    "484": "Always Be My Baby",
    "485": "Biggest Part of Me",
    "486": "Eye In The Sky",
    "487": "Lean on Me",
    "488": "Waiting For A Girl Like You",
    "489": "Lyin' Eyes",
    "490": "The Long And Winding Road",
    "491": "Sailing",
    "492": "Man in the Mirror",
    "493": "Lowdown",
    "494": "If Ever You're In My Arms Again",
    "495": "Hold On",
    "496": "Baby, I Love Your Way",
    "497": "Baby I'm-A Want You",
    "498": "Making Love Out of Nothing At All",
    "499": "Baby, I Love Your Way",
    "500": "Rolling in the Deep",
    "501": "Try",
    "502": "Pompeii",
    "503": "Dude (Looks Like A Lady)",
    "504": "Young Girls",
    "505": "Walk Away",
    "506": "Livin' On a Prayer",
    "507": "Makes Me Wonder",
    "508": "I Gotta Feeling",
    "509": "Stop and Stare",
    "510": "When I Come Around",
    "511": "Umbrella",
    "512": "Real World",
    "513": "Move Along",
    "514": "Clarity",
    "515": "Cruise",
    "516": "You Get What You Give",
    "517": "Starlight",
    "518": "Timber",
    "519": "Me and My Broken Heart",
    "520": "Big Empty",
    "521": "Not a Bad Thing",
    "522": "Little Lion Man",
    "523": "Dreams",
    "524": "Without You",
    "525": "Please Don't Leave Me",
    "526": "Use Somebody",
    "527": "If You Don't Know Me By Now",
    "528": "I Honestly Love You",
    "529": "People Are People",
    "530": "Bakit Ikaw Pa",
    "533": "California Gurls",
    "534": "Love Bites",
    "535": "One More Night",
    "541": "Wake Me Up When September Ends",
    "551": "Hindi Ko Kaya",
    "552": "Policy of Truth",
    "554": "Hysteria",
    "562": "Lalaki,Ikaw ang Dahilan",
    "572": "Huwag",
    "700": "My Life",
    "701": "Can't Get Enough",
    "702": "Hotel California",
    "703": "More Than a Feeling",
    "704": "Make It With You",
    "705": "Can't Get It Out of My Head",
    "706": "One of These Nights",
    "707": "Another Brick in the Wall",
    "708": "Hooked On a Feeling",
    "709": "Second Hand News",
    "710": "Feels Like the First Time",
    "711": "Hot Blooded",
    "712": "Before the Next Tear Drop Falls",
    "713": "Bad, Bad Leroy Brown",
    "714": "Rocky Mountain Way",
    "715": "Point of Not Return",
    "716": "Black Dog",
    "717": "In The Evening",
    "718": "Cat's In The Cradle",
    "719": "Shine On You Crazy Diamonds",
    "720": "Alight Shade of Pale",
    "721": "Rock & Roll, Hoochie Koo",
    "722": "True Blue",
    "723": "Try And Love Again",
    "724": "Babe",
    "725": "I Robot",
    "726": "China Grove",
    "727": "Miss You",
    "728": "On The Border",
    "729": "Strange Magic",
    "730": "You Make Loving Fun",
    "731": "Peace of Mind",
    "732": "School's Out",
    "733": "Cold as Ice",
    "734": "Truckin'",
    "735": "Rock and Roll",
    "736": "Tonight's the Night",
    "737": "Spirit in the Sky",
    "738": "Show Me the Way",
    "739": "Welcome to the Machine",
    "740": "We are the Champions",
    "741": "Lady",
    "742": "I Wouldn't Want to be Like You",
    "743": "Ramblin Man",
    "744": "Long Train Running",
    "745": "Big Shot",
    "746": "Life in the Fast Lane",
    "747": "What's On My Mind",
    "748": "Have a Cigar",
    "749": "Time",
    "750": "Colour My World",
    "751": "I Want You to Want Me",
    "752": "Rich Girl",
    "753": "Don't Stop",
    "754": "American Pie",
    "755": "Venus",
    "756": "Rock N Me",
    "757": "Takin' Care of Business",
    "758": "Dream On",
    "759": "Carry On Wayward Son",
    "760": "Don't Bring Me Down",
    "761": "Band On the Run",
    "762": "Money",
    "763": "First Cut is The Deepest",
    "764": "American Woman",
    "765": "Stairway To Heaven",
    "766": "If You Leave Me Now",
    "767": "Victim of Love",
    "768": "Philadelphia Freedom",
    "769": "Double Vision",
    "770": "You Don't Bring Me Flowers",
    "771": "The Joker",
    "772": "You Ain't Seen Nothing Yet",
    "773": "Walk This Way",
    "774": "Island Girl",
    "775": "All Right Now",
    "776": "Dust In The Wind",
    "777": "Fly Like An Eagle",
    "778": "Long Cool Woman (In a Black Dress)",
    "779": "My Sharona",
    "780": "Won't Get Fooled Again",
    "781": "Teacher",
    "782": "Lido Shuffle",
    "783": "Longfellow Serenade",
    "784": "My Love",
    "785": "Hold the Line",
    "786": "Heart of Glass",
    "787": "Smokin' in the Boys Room",
    "788": "Frankenstein",
    "789": "Kiss You All Over",
    "790": "We're An American Band",
    "791": "Your Mama Don't Dance",
    "792": "Conquistador",
    "793": "You've Got a Friend",
    "794": "I Am…I Said",
    "795": "How Much I Feel",
    "796": "Let It Be",
    "797": "Strange Way",
    "798": "Green-Eyed Lady",
    "799": "Lay Down Sally"
}

var handlers = {
    'GetSongRequested': function() {

        console.log("intent " + this.event.request.intent.slots.Song.value);

        var song_request = this.event.request.intent.slots.Song.value;
        var speechOutput = "Your song " + song_request + " was not found.";
        var cardTitle = "Jukebox - Song Request";
        var found_song = false;
        console.log("INTENT -> " + song_request);

        // find song in song list
        for (var key in SONGS) {
            if (SONGS[key].toLowerCase() == song_request.toLowerCase()) {
                console.log(key + " -> " + SONGS[key]);

                // Create speech output
                speechOutput = "Sending song number " + key + ", " + song_request + ", to the jukebox.";
                found_song = true;
                final_song_key = key
            }
        }

        var _this = this;

        if (found_song) {

            sendQueueMessage({
                request_type: 'GetSongRequested',
                parameters: {
                  key: final_song_key,
                  message_body: speechOutput
                }
            }, function(error) {
                if (error) {
                    console.log(error);
                    _this.emit(':tell', 'Sorry, something went wrong.');
                } else {
                    _this.emit(':tellWithCard', speechOutput, cardTitle);
                }
            });
        } else {
            _this.emit(':tellWithCard', speechOutput, cardTitle);
        }
    },

    'GetSongIdRequested': function() {

        console.log("intent " + this.event.request.intent.slots.SongNumber.value);

        var song_request = this.event.request.intent.slots.SongNumber.value;
        var speechOutput = "Your song number " + song_request + " was not found.";
        var cardTitle = "Jukebox - Song Id Request";
        var found_song = false;
        console.log("INTENT -> " + song_request);

        // find song in song list
        for (var key in SONGS) {
          if (SONGS.hasOwnProperty(key)) {
            if (key == song_request) {
                console.log(key + " -> " + SONGS[key]);

                // Create speech output
                speechOutput = "Sending song number " + key + ", " + SONGS[key] + ", to the jukebox.";
                final_song_key = key
                found_song = true;
            }
          }
        }

        var _this = this;

        if (found_song) {

            sendQueueMessage({
                request_type: 'GetSongIdRequested',
                parameters: {
                  key: final_song_key,
                  message_body: speechOutput
                }
            }, function(error) {
                if (error) {
                    console.log(error);
                    _this.emit(':tell', 'Sorry, something went wrong.');
                } else {
                    _this.emit(':tellWithCard', speechOutput, cardTitle);
                }
            });
        } else {
            _this.emit(':tellWithCard', speechOutput, cardTitle);
        }
    },
    'SpeakerRequest': function() {
        console.log("intent " + this.event.request.intent.slots.Song.value);
        var speaker_request = this.event.request.intent.slots.Song.value;
        var speechOutput = "Turning speaker " + speaker_request;
        var cardTitle = "Jukebox - Speaker Request";
        console.log("INTENT -> " + speaker_request);
        var _this = this;
        sendQueueMessage({
            request_type: 'SpeakerReqeust',
            parameters: {
              key: speaker_request,
              message_body: speechOutput
            }
        }, function(error) {
            if (error) {
                console.log(error);
                _this.emit(':tell', 'Sorry, something went wrong.');
            } else {
                _this.emit(':tellWithCard', speechOutput, cardTitle);
            }
        });
    },

}

exports.handler = function(event, context, callback) {
    console.log('started');

    console.log(event);
    if (event.session.application.applicationId !== appId) {
        context.fail("Invalid Application ID");
    }

    var alexa = Alexa.handler(event, context);
    alexa.appId = appId;
    alexa.registerHandlers(handlers);
    alexa.execute();
}

function sendQueueMessage(payload, callback) {

    console.log('message payload: ' + JSON.stringify(payload));
    var SQS = new AWS.SQS();

    var parameters = {
        MessageBody: JSON.stringify(payload),
        QueueUrl: sqsQueueUrl,
        MessageGroupId:'Song',
        MessageDeduplicationId: Math.round(new Date().getTime()/1000).toString()
    };

    SQS.sendMessage(parameters, callback);
}
