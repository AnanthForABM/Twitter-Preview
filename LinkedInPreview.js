function Post() {
    var post = document.getElementById('postTextArea').value;
    var urlRegex = /(http:\/\/www\.|https:\/\/www\.|http:\/\/|https:\/\/)?[a-z0-9]+([\-\.]{1}[a-z0-9]+)*\.[a-z]{2,5}(:[0-9]{1,5})?(\/[^\s]*)?/g;
    debugger;
    var urlArray = post.match(urlRegex);
    var urlParsedTweet = encodeURIComponent(post.replace(urlRegex, ""));

    if (urlArray.length > 1)
        var serviceURL = '/Home/PostShare?text=' + post;
    else if (urlArray.length > 0)
        serviceURL = '/Home/PostShare?url=' + urlArray[0] + '&text=' + urlParsedTweet;
    else
        serviceURL = '/Home/PostShare?text=' + urlParsedTweet;

    $.ajax({
        type: "POST",
        url: serviceURL,
        data: param = "",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: successFunc,
        error: errorFunc
    });

    function successFunc(data, status) {
        alert(data);
    }

    function errorFunc() {
        alert('error');
    }
}

function Parse() {
    document.getElementById('linkedInPreviewBody').innerHTML = "";
    var post = document.getElementById('postTextArea').value;
    var urlRegex = /(http:\/\/www\.|https:\/\/www\.|http:\/\/|https:\/\/)?[a-z0-9]+([\-\.]{1}[a-z0-9]+)*\.[a-z]{2,5}(:[0-9]{1,5})?(\/[^\s]*)?/g;
    debugger;
    var urlArray = post.match(urlRegex);
    if (urlArray.length == 1) {
        var urlParsedTweet = post.replace(urlRegex, "")
    }
    else {
        var urlParsedTweet = post.replace(urlRegex, url => {
            return '<a href="' + url + '" class="urlText"> https://lnkd.in/f9bnBzh </a>';
        });
    }
    var hashRegex = /\B(#[a-zA-z0-9]+\b)/g;
    var hashParsedTweet = urlParsedTweet.replace(hashRegex, function (url) {
        return '<a href="' + url + '" class="hashtagText">' + url + '</a>';
    })
    var div = document.createElement('div');
    div.innerHTML = hashParsedTweet;
    div.setAttribute("class", "linkedInPreview--bodyTextArea");
    document.getElementById('linkedInPreviewBody').appendChild(div);

    if (urlArray.length > 0) {
        var serviceURL = '/Home/GetMetaData?url=' + urlArray[0]; 
        $.ajax({
            type: "POST",
            url: serviceURL,
            data: param = "",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: successFunc,
            error: errorFunc
        });

        function successFunc(data) {
            CreateLinkedInCard(data, urlArray[0]);
        }

        function errorFunc() {
            alert('error');
        }
    } 
}

function CreateLinkedInCard(data,url) {
    var obj = data

    var img = document.createElement("img");
    img.setAttribute("src", obj["og:image"]);
    console.log(img.height + "," + img.width);
    var imgDiv = document.createElement("div");
    imgDiv.appendChild(img);
    var titleHeading = document.createElement("h2");
    titleHeading.innerHTML = HtmlParse(obj["og:title"]);
    titleHeading.setAttribute("class", "linkedInPreview-cardTitle");
    var domainSpan = document.createElement("span");

    domainSpan.setAttribute("class", "linkedInPreview-cardDomain");
    var summaryCardDiv = document.createElement("div");
    summaryCardDiv.appendChild(titleHeading);
    summaryCardDiv.appendChild(domainSpan);

    img.onload = () => ImageResize(img, 522, 288);
    imgDiv.setAttribute("class", "linkedInPreview-summaryLageCardImageDiv");
    summaryCardDiv.setAttribute("class", "linkedInPreview-summaryLargeCardContent");


    var linkedInCardAnchor = document.createElement("a");
    linkedInCardAnchor.setAttribute("href", url);
    linkedInCardAnchor.setAttribute("class", "linkedInPreview-cardAnchor");
    linkedInCardAnchor.appendChild(imgDiv);
    linkedInCardAnchor.appendChild(summaryCardDiv);
    domainSpan.innerHTML = linkedInCardAnchor.hostname;
    var linkedInCardDiv = document.createElement("div");
    linkedInCardDiv.appendChild(linkedInCardAnchor);
    document.getElementById("linkedInPreviewBody").appendChild(linkedInCardDiv);
}
function HtmlParse(encodedStr) {
    var parser = new DOMParser;
    var dom = parser.parseFromString('<!doctype html><body>' + encodedStr, 'text/html');
    return dom.body.textContent;
}

function ImageResize(image, targetWidth, targetHeight) {
    debugger;
    var width = image.naturalWidth;
    var height = image.naturalHeight;
    image.width = targetWidth;
    var corsHeight = (targetWidth / width) * height;
    image.height = corsHeight;
    image.style.transform = 'translatey(-' + parseInt((corsHeight - targetHeight) / 2) + 'px)';
    image.style.webkitTransform = 'translateY(-' + parseInt((corsHeight - targetHeight) / 2) + 'px)';
}	
