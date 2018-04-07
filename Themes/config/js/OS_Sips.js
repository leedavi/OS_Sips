

$(document).ready(function () {

    $('#sips1_cmdSave').unbind("click");
    $('#sips1_cmdSave').click(function () {
        $('.processing').show();
        $('.actionbuttonwrapper').hide();
        // lower case cmd must match ajax provider ref.
        nbxget('sips1_savesettings', '.OS_Sipsdata', '.OS_Sipsreturnmsg');
    });

    $('.selectlang').unbind("click");
    $(".selectlang").click(function () {
        $('.editlanguage').hide();
        $('.actionbuttonwrapper').hide();
        $('.processing').show();
        $("#nextlang").val($(this).attr("editlang"));
        // lower case cmd must match ajax provider ref.
        nbxget('sips1_selectlang', '.OS_Sipsdata', '.OS_Sipsdata');
    });


    $(document).on("nbxgetcompleted", sips1_nbxgetCompleted); // assign a completed event for the ajax calls

    // function to do actions after an ajax call has been made.
    function sips1_nbxgetCompleted(e) {

        $('.processing').hide();
        $('.actionbuttonwrapper').show();
        $('.editlanguage').show();

        if (e.cmd == 'sips1_selectlang') {
                        
        }

    };

});

