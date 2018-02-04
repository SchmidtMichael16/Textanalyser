// wait for the DOM to be loaded
$(function () {
    // bind 'myForm' and provide a simple callback function
    //$('#SearchForm').ajaxForm(function () {
    //    alert("Thank you for your comment!");
    //});



});

function SearchWords() {
    var xhttp = new XMLHttpRequest();
    //xhttp.onreadystatechange = function () {
    //    if (this.readyState == 4 && this.status == 200) {
    //        if (this.responseText != "")
    //        {
    //            var liElement = document.createElement('li');
    //            liElement.innerHTML = this.responseText;
    //            document.getElementById('Results').appendChild(liElement);
    //        }

    //    }
    //};

    var word1 = document.getElementById('Word1');
    var word2 = document.getElementById('Word2');
    var word3 = document.getElementById('Word3');
    var alsoSynonyms = "false";

    if (document.getElementById('AlsoSynonyms').checked) {
        alsoSynonyms = "true";
    }


    xhttp.open("GET", "../api/search?Word1= " + word1.value + "&Word2=" + word2.value + "&Word3=" + word3.value + "&AlsoSynonyms=" + alsoSynonyms, true);
    xhttp.send(); 
}