
CommentBodyId = "#comment"
var product = -1
function imageBox(smallImg) {
    var fullImage = document.getElementById("image-box")
    fullImage.src = smallImg.src
}

$(document).ready(function () {
    var url = $("#comment").data("url")
    $("#comment").load(url)
    productId = $("#comment").data("product-id")
    $(CommentBodyId).load("/Comment/ShowProductComments?id="+productId)
})

function doComment(btn,e,commentId,spanId)
{
    var button = $(btn)
    var mode = button.data("edit-mode")
    var editableContent = $("#comment_text_" + commentId)


    if (e == "new_clicked") {
        var txt = $("#new_comment_text").val()

        $.ajax({
            method: "POST",
            url: "/Comment/Create",
            data: { 'text': txt, 'productId': productId }
        })
            .done(function (data) {
                if (data.result) {
                    $(CommentBodyId).load("/Comment/ShowProductComments?id=" + productId)
                }
                else {
                    alert("Yorum yapılırken bir hata oluştu!");
                }
            })
            .fail(function (error) {
                alert("Sunucuda bir hata oluştu!")
            })

    }
    else if (e == "delete_clicked") {
        var dialog_res = confirm("Yorumu silmek istediğinize emin misiniz?")
        if (!dialog_res) return false;

        $.ajax({
            method: "POST",
            url: "/Comment/Delete?id=" + commentId,
        })
        .done(function (data) {
            if (data.result) {
                $(CommentBodyId).load("/Comment/ShowProductComments?id=" + productId)
            }
            else {
                alert("Yorum silinirken bir hata oluştu!");
            }
        })
        .fail(function (error) {
            alert("Sunucuda bir hata oluştu!")
        })
    }
}