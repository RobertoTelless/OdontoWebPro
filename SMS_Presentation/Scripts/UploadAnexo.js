function getUrlParameter(sParam) {
    var sPageURL = window.location.search.substring(1),
        sURLVariables = sPageURL.split('&'),
        sParameterName,
        i;

    for (i = 0; i < sURLVariables.length; i++) {
        sParameterName = sURLVariables[i].split('=');

        if (sParameterName[0] === sParam) {
            return sParameterName[1] === undefined ? true : decodeURIComponent(sParameterName[1]);
        }
    }
}

function SendFiles(inputProfileId, controller, actionIncluir, actionUpload, actionMontarTela, editar = false) {
    controller = '../' + controller + '/';

    let inputFile = $('#inputFile');
    let inputProfile = $('#' + inputProfileId); //$('#imgExemplo');
    let buttonSubmit = $('.btnSubmit');
    let filesContainer = $('#myFiles');
    let files = [];

    inputFile.change(function () {
        let newFiles = [];
        for (let index = 0; index < inputFile[0].files.length; index++) {
            let file = inputFile[0].files[index];
            newFiles.push(file);
            files.push(file);
        }

        newFiles.forEach(file => {
            let fileElement = $(`<tr><td>${file.name}</td><td><span class="tbl-link fa-lg fa fa-file"></span></td><td><span class="tbl-link fa-lg fa fa-trash-o"></span></td></tr>`);
            fileElement.data('fileData', file);
            $('.dataTables_empty').parent().remove();
            filesContainer.append(fileElement);

            fileElement.click(function (event) {
                let fileElement = $(event.target);
                let indexToRemove = files.indexOf(fileElement.data('fileData'));
                fileElement.parent().parent().remove();
                files.splice(indexToRemove, 1);
            });
        });
    });

    inputProfile.change(function () {
        if ($('#profile').length == 0) {
            let newFiles = [];
            for (let index = 0; index < inputProfile[0].files.length; index++) {
                let file = inputProfile[0].files[index];
                newFiles.push(file);
                files.push(file);
            };

            newFiles.forEach(file => {
                let fileElement = $(`<tr><td id="profile">${file.name}</td><td><span class="tbl-link fa-lg fa fa-user-circle"></span></td><td class="td-one-action"><span class="tbl-link fa-lg fa fa-trash-o"></span></td></tr>`);
                fileElement.data('fileData', file);
                $('.dataTables_empty').parent().remove()
                filesContainer.prepend(fileElement);

                fileElement.click(function (event) {
                    let fileElement = $(event.target);
                    let indexToRemove = files.indexOf(fileElement.data('fileData'));
                    fileElement.parent().parent().remove();
                    files.splice(indexToRemove, 1);
                });
            });
        } else {
            alert('Foto de perfil ja inclusa');
        }
    });

    buttonSubmit.click(function () {
        //toastr.success('Inclusão em andamento!')

        let formData = new FormData();

        files.forEach(file => {
            formData.append('files', file);
        });

        console.log('Sending...');

        $.ajax({
            url: controller + actionIncluir //'../Exemplo/IncluirExemplo'
            , data: $('#pwd-container1').serializeArray()
            , type: 'POST'
            , success: function (r) {
                if ($('#profile').length == 0) {
                    formData.append('perfil', 0);
                } else {
                    formData.append('perfil', 1);
                }

                if (editar || r.id != undefined) {
                    if (files.length > 0) {
                        $.ajax({
                            url: controller + actionUpload //'../Exemplo/UploadFileExemplo_Inclusao'
                            , async: false
                            , data: formData
                            , type: 'POST'
                            , success: function (data) {
                                if (getUrlParameter('voltaCliente') == "1") {
                                    window.open('../Atendimento/IncluirAtendimento', '_self');
                                } else {
                                    if (editar) {
                                        window.open(controller + actionMontarTela + '/' + r.id, '_self');
                                    } else {
                                        window.open(controller + actionMontarTela, '_self');
                                    }
                                }
                            } //'../Exemplo/MontarTelaExemplo'
                            , error: function (data) { console.log('ERROR!!'); }
                            , cache: false
                            , processData: false
                            , contentType: false
                        });
                    } else {
                        if (getUrlParameter('voltaCliente') == "1") {
                            window.open('../Atendimento/IncluirAtendimento', '_self');
                        } else {
                            if (editar && r.id != undefined) {
                                window.open(controller + actionMontarTela + '/' + r.id, '_self');
                            } else if (r.error != undefined) {
                                $('.ibox-content').find('.alert').remove();
                                $('.ibox-content').prepend(
                                    '<div class="alert alert-danger">'
                                    + '<span>' + r.error + '</span>'
                                    + '<button type="button" class="close" data-dismiss="alert">Fechar</button>'
                                    + '</div>');
                            } else {
                                window.open(controller + actionMontarTela, '_self');
                            }
                        }
                    }
                }
                else {
                    if (r.error == undefined) {
                        var alert = $(r).find('.alert');
                        $('.ibox-content').find('.alert').remove();
                        $('.ibox-content').prepend(alert);
                    }
                    else {
                        $('.ibox-content').find('.alert').remove();
                        $('.ibox-content').prepend(
                            '<div class="alert alert-danger">'
                            + '<span>' + r.error + '</span>'
                            + '<button type="button" class="close" data-dismiss="alert">Fechar</button>'
                            + '</div>');
                    }
                }
            }
        });
    });
}


function SendFilesCompra(inputProfileId, controller, actionIncluir, actionUpload, actionMontarTela, editar = false) {
    controller = '../' + controller + '/';

    let inputFile = $('#inputFile');
    let inputProfile = $('#' + inputProfileId); //$('#imgExemplo');
    let buttonSubmit = $('.btnSubmit');
    let filesContainer = $('#myFiles');
    let files = [];

    inputFile.change(function () {
        let newFiles = [];
        for (let index = 0; index < inputFile[0].files.length; index++) {
            let file = inputFile[0].files[index];
            newFiles.push(file);
            files.push(file);
        }

        newFiles.forEach(file => {
            let fileElement = $(`<tr><td>${file.name}</td><td><span class="tbl-link fa-lg fa fa-file"></span></td><td><span class="tbl-link fa-lg fa fa-trash-o"></span></td></tr>`);
            fileElement.data('fileData', file);
            $('.dataTables_empty').parent().remove();
            filesContainer.append(fileElement);

            fileElement.click(function (event) {
                let fileElement = $(event.target);
                let indexToRemove = files.indexOf(fileElement.data('fileData'));
                fileElement.parent().parent().remove();
                files.splice(indexToRemove, 1);
            });
        });
    });

    inputProfile.change(function () {
        if ($('#profile').length == 0) {
            let newFiles = [];
            for (let index = 0; index < inputProfile[0].files.length; index++) {
                let file = inputProfile[0].files[index];
                newFiles.push(file);
                files.push(file);
            };

            newFiles.forEach(file => {
                let fileElement = $(`<tr><td id="profile">${file.name}</td><td><span class="tbl-link fa-lg fa fa-user-circle"></span></td><td class="td-one-action"><span class="tbl-link fa-lg fa fa-trash-o"></span></td></tr>`);
                fileElement.data('fileData', file);
                $('.dataTables_empty').parent().remove()
                filesContainer.prepend(fileElement);

                fileElement.click(function (event) {
                    let fileElement = $(event.target);
                    let indexToRemove = files.indexOf(fileElement.data('fileData'));
                    fileElement.parent().parent().remove();
                    files.splice(indexToRemove, 1);
                });
            });
        } else {
            alert('Foto de perfil ja inclusa');
        }
    });

    buttonSubmit.click(function () {
        //toastr.success('Inclusão em andamento!')

        let formData = new FormData();

        files.forEach(file => {
            formData.append('files', file);
        });

        console.log('Sending...');

        var model = {
            "ASSI_CD_ID": $('#ASSI_CD_ID').val()
            , "PECO_IN_ATIVO": $('#PECO_IN_ATIVO').val()
            , "PECO_IN_STATUS": $('#PECO_IN_STATUS').val()
            , "MATR_CD_ID": $('#MATR_CD_ID').val()
            , "FILI_CD_ID": $('#FILI_CD_ID').val()
            , "USUA_CD_ID": $('#USUA_CD_ID').val()
            , "PECO_NM_NOME": $('#PECO_NM_NOME').val()
            , "PECO_DT_DATA": $('#PECO_DT_DATA').val()
            , "PECO_DT_PREVISTA": $('#PECO_DT_PREVISTA').val()
            , "PECO_DS_DESCRICAO": $('#PECO_DS_DESCRICAO').val()
            , "PECO_CD_ID": $('#PECO_CD_ID').val()
            , "PECO_TX_OBSERVACAO": $('#PECO_TX_OBSERVACAO').val()
        }

        var json = new Array();

        $('table#itemPedido > tbody > tr').each(function (i, e) {
            json.push({
                "ITPC_IN_TIPO": $(e).find('input[name="rowTipo"]').val()
                , "PROD_CD_ID": $(e).find('input[name="rowProd"]').val()
                , "MAPR_CD_ID": $(e).find('input[name="rowIns"]').val()
                , "UNID_CD_ID": $(e).find('input[name="rowUnd"]').val()
                , "ITPC_QN_QUANTIDADE": $(e).find('input[name="rowQtde"]').val()
                , "ITPC_TX_OBSERVACOES": $(e).find('input[name="rowObs"]').val()
            });
        });

        var data = {
            "vm": model
            , "tabelaItemPedido": JSON.stringify(json)
        }

        $.ajax({
            url: controller + actionIncluir //'../Exemplo/IncluirExemplo'
            , data: data
            , type: 'POST'
            , success: function (r) {
                if ($('#profile').length == 0) {
                    formData.append('perfil', 0);
                } else {
                    formData.append('perfil', 1);
                }

                if (files.length > 0) {
                    $.ajax({
                        url: controller + actionUpload //'../Exemplo/UploadFileExemplo_Inclusao'
                        , async: false
                        , data: formData
                        , type: 'POST'
                        , success: function (data) {
                            if (editar) {
                                window.open(controller + actionMontarTela + '/' + r, '_self');
                            } else {
                                window.open(controller + actionMontarTela, '_self');
                            }
                        } //'../Exemplo/MontarTelaExemplo'
                        , error: function (data) { console.log('ERROR!!'); }
                        , cache: false
                        , processData: false
                        , contentType: false
                    });
                } else {
                    if (editar) {
                        window.open(controller + actionMontarTela + '/' + r, '_self');
                    } else {
                        window.open(controller + actionMontarTela, '_self');
                    }
                }
            }
        });
    });
}

function SendFilesUsuario(inputProfileId, controller, actionIncluir, actionUpload, actionMontarTela, editar = false) {
    controller = '../' + controller + '/';

    let inputFile = $('#inputFile');
    let inputProfile = $('#' + inputProfileId); //$('#imgExemplo');
    let buttonSubmit = $('.btnSubmit');
    let filesContainer = $('#myFiles');
    let files = [];

    inputFile.change(function () {
        let newFiles = [];
        for (let index = 0; index < inputFile[0].files.length; index++) {
            let file = inputFile[0].files[index];
            newFiles.push(file);
            files.push(file);
        }

        newFiles.forEach(file => {
            let fileElement = $(`<tr><td>${file.name}</td><td><span class="tbl-link fa-lg fa fa-file"></span></td><td><span class="tbl-link fa-lg fa fa-trash-o"></span></td></tr>`);
            fileElement.data('fileData', file);
            $('.dataTables_empty').parent().remove();
            filesContainer.append(fileElement);

            fileElement.click(function (event) {
                let fileElement = $(event.target);
                let indexToRemove = files.indexOf(fileElement.data('fileData'));
                fileElement.parent().parent().remove();
                files.splice(indexToRemove, 1);
            });
        });
    });

    inputProfile.change(function () {
        if ($('#profile').length == 0) {
            let newFiles = [];
            for (let index = 0; index < inputProfile[0].files.length; index++) {
                let file = inputProfile[0].files[index];
                newFiles.push(file);
                files.push(file);
            };

            newFiles.forEach(file => {
                let fileElement = $(`<tr><td id="profile">${file.name}</td><td><span class="tbl-link fa-lg fa fa-user-circle"></span></td><td class="td-one-action"><span class="tbl-link fa-lg fa fa-trash-o"></span></td></tr>`);
                fileElement.data('fileData', file);
                $('.dataTables_empty').parent().remove()
                filesContainer.prepend(fileElement);

                fileElement.click(function (event) {
                    let fileElement = $(event.target);
                    let indexToRemove = files.indexOf(fileElement.data('fileData'));
                    fileElement.parent().parent().remove();
                    files.splice(indexToRemove, 1);
                });
            });
        } else {
            alert('Foto de perfil ja inclusa');
        }
    });

    buttonSubmit.click(function () {
        //toastr.success('Inclusão em andamento!')

        let formData = new FormData();

        files.forEach(file => {
            formData.append('files', file);
        });

        console.log('Sending...');

        $.ajax({
            url: controller + actionIncluir //'../Exemplo/IncluirExemplo'
            , data: $('#pwd-container1').serializeArray()
            , type: 'POST'
            , success: function (r) {
                if ($('#profile').length == 0) {
                    formData.append('perfil', 0);
                } else {
                    formData.append('perfil', 1);
                }

                if (editar || r == 1) {
                    if (files.length > 0) {
                        $.ajax({
                            url: controller + actionUpload //'../Exemplo/UploadFileExemplo_Inclusao'
                            , async: false
                            , data: formData
                            , type: 'POST'
                            , success: function (data) {
                                if (editar) {
                                    window.open(controller + actionMontarTela + '/' + r, '_self');
                                } else {
                                    window.open(controller + actionMontarTela, '_self');
                                }
                            } //'../Exemplo/MontarTelaExemplo'
                            , error: function (data) { console.log('ERROR!!'); }
                            , cache: false
                            , processData: false
                            , contentType: false
                        });
                    } else {
                        if (editar) {
                            window.open(controller + actionMontarTela + '/' + r, '_self');
                        } else {
                            window.open(controller + actionMontarTela, '_self');
                        }
                    }
                } else {
                    $('.tabs-container').prepend(
                        '<div class="alert alert-danger">'
                        + '<span>' + r + '</span>'
                        + '<button type="button" class="close" data-dismiss="alert">Fechar</button>'
                        + '</div>');
                }
            }
        });
    });
}

function SendFilesAgenda(inputProfileId, controller, actionIncluir, actionUpload, actionMontarTela, editar = false) {
    controller = '../' + controller + '/';

    let inputFile = $('#inputFile');
    let inputProfile = $('#' + inputProfileId); //$('#imgExemplo');
    let buttonSubmit = $('.btnSubmit');
    let filesContainer = $('#myFiles');
    let files = [];

    inputFile.change(function () {
        let newFiles = [];
        for (let index = 0; index < inputFile[0].files.length; index++) {
            let file = inputFile[0].files[index];
            newFiles.push(file);
            files.push(file);
        }

        newFiles.forEach(file => {
            let fileElement = $(`<tr><td>${file.name}</td><td><span class="tbl-link fa-lg fa fa-file"></span></td><td><span class="tbl-link fa-lg fa fa-trash-o"></span></td></tr>`);
            fileElement.data('fileData', file);
            $('.dataTables_empty').parent().remove();
            filesContainer.append(fileElement);

            fileElement.click(function (event) {
                let fileElement = $(event.target);
                let indexToRemove = files.indexOf(fileElement.data('fileData'));
                fileElement.parent().parent().remove();
                files.splice(indexToRemove, 1);
            });
        });
    });

    inputProfile.change(function () {
        if ($('#profile').length == 0) {
            let newFiles = [];
            for (let index = 0; index < inputProfile[0].files.length; index++) {
                let file = inputProfile[0].files[index];
                newFiles.push(file);
                files.push(file);
            };

            newFiles.forEach(file => {
                let fileElement = $(`<tr><td id="profile">${file.name}</td><td><span class="tbl-link fa-lg fa fa-user-circle"></span></td><td class="td-one-action"><span class="tbl-link fa-lg fa fa-trash-o"></span></td></tr>`);
                fileElement.data('fileData', file);
                $('.dataTables_empty').parent().remove()
                filesContainer.prepend(fileElement);

                fileElement.click(function (event) {
                    let fileElement = $(event.target);
                    let indexToRemove = files.indexOf(fileElement.data('fileData'));
                    fileElement.parent().parent().remove();
                    files.splice(indexToRemove, 1);
                });
            });
        } else {
            alert('Foto de perfil ja inclusa');
        }
    });

    buttonSubmit.click(function () {
        //toastr.success('Inclusão em andamento!')

        var model = {
            "ASSI_CD_ID": $('#cdAssi').val()
            , "AGEN_IN_ATIVO": $('#ativo').val()
            , "USUA_CD_ID": $('#cdUsua').val()
            , "AGEN_IN_STATUS": $('#status').val()
            , "AGEN_DT_DATA": $('#data').val()
            , "AGEN_HR_HORA": $('#hora').val()
            , "CAAG_CD_ID": $('#catAgenda').val()
            , "AGEN_CD_USUARIO": $('#cdUsuAgenda').val()
            , "AGEN_NM_TITULO": $('#titulo').val()
            , "AGEN_DS_DESCRICAO": $('#desc').val()
            , "AGEN_TX_OBSERVACOES": $('#obs').val()
        }

        let formData = new FormData();
        var data = {
            vm: model
        }

        files.forEach(file => {
            formData.append('files', file);
        });

        console.log('Sending...');

        $.ajax({
            url: controller + actionIncluir //'../Exemplo/IncluirExemplo'
            , data: data
            , type: 'POST'
            , success: function (r) {
                if ($('#profile').length == 0) {
                    formData.append('perfil', 0);
                } else {
                    formData.append('perfil', 1);
                }

                if (files.length > 0) {
                    $.ajax({
                        url: controller + actionUpload //'../Exemplo/UploadFileExemplo_Inclusao'
                        , async: false
                        , data: formData
                        , type: 'POST'
                        , success: function (data) {
                            if (editar) {
                                window.open(controller + actionMontarTela + '/' + r, '_self');
                            } else {
                                window.open(controller + actionMontarTela, '_self');
                            }
                        } //'../Exemplo/MontarTelaExemplo'
                        , error: function (data) { console.log('ERROR!!'); }
                        , cache: false
                        , processData: false
                        , contentType: false
                    });
                } else {
                    if (editar) {
                        window.open(controller + actionMontarTela + '/' + r, '_self');
                    } else {
                        window.open(controller + actionMontarTela, '_self');
                    }
                }
            }
        });
    });
}

function SendFilesAgenda(inputProfileId, controller, actionIncluir, actionUpload, actionMontarTela, editar = false) {
    controller = '../' + controller + '/';

    let inputFile = $('#inputFile');
    let inputProfile = $('#' + inputProfileId); //$('#imgExemplo');
    let buttonSubmit = $('.btnSubmit');
    let filesContainer = $('#myFiles');
    let files = [];

    inputFile.change(function () {
        let newFiles = [];
        for (let index = 0; index < inputFile[0].files.length; index++) {
            let file = inputFile[0].files[index];
            newFiles.push(file);
            files.push(file);
        }

        newFiles.forEach(file => {
            let fileElement = $(`<tr><td>${file.name}</td><td><span class="tbl-link fa-lg fa fa-file"></span></td><td><span class="tbl-link fa-lg fa fa-trash-o"></span></td></tr>`);
            fileElement.data('fileData', file);
            $('.dataTables_empty').parent().remove();
            filesContainer.append(fileElement);

            fileElement.click(function (event) {
                let fileElement = $(event.target);
                let indexToRemove = files.indexOf(fileElement.data('fileData'));
                fileElement.parent().parent().remove();
                files.splice(indexToRemove, 1);
            });
        });
    });

    inputProfile.change(function () {
        if ($('#profile').length == 0) {
            let newFiles = [];
            for (let index = 0; index < inputProfile[0].files.length; index++) {
                let file = inputProfile[0].files[index];
                newFiles.push(file);
                files.push(file);
            };

            newFiles.forEach(file => {
                let fileElement = $(`<tr><td id="profile">${file.name}</td><td><span class="tbl-link fa-lg fa fa-user-circle"></span></td><td class="td-one-action"><span class="tbl-link fa-lg fa fa-trash-o"></span></td></tr>`);
                fileElement.data('fileData', file);
                $('.dataTables_empty').parent().remove()
                filesContainer.prepend(fileElement);

                fileElement.click(function (event) {
                    let fileElement = $(event.target);
                    let indexToRemove = files.indexOf(fileElement.data('fileData'));
                    fileElement.parent().parent().remove();
                    files.splice(indexToRemove, 1);
                });
            });
        } else {
            alert('Foto de perfil ja inclusa');
        }
    });

    buttonSubmit.click(function () {
        //toastr.success('Inclusão em andamento!')

        let formData = new FormData();

        files.forEach(file => {
            formData.append('files', file);
        });

        console.log('Sending...');

        $.ajax({
            url: controller + actionIncluir //'../Exemplo/IncluirExemplo'
            , data: $('#pwd-container1').serializeArray()
            , type: 'POST'
            , success: function (r) {
                if (r.idAtendimento != undefined) {
                    editar = true;
                    r.id = r.idAtendimento;
                    controller = "../Atendimento/";
                    actionMontarTela = "EditarAtendimento";
                }

                if ($('#profile').length == 0) {
                    formData.append('perfil', 0);
                } else {
                    formData.append('perfil', 1);
                }

                if (editar || r.id != undefined) {
                    if (files.length > 0) {
                        $.ajax({
                            url: controller + actionUpload //'../Exemplo/UploadFileExemplo_Inclusao'
                            , async: false
                            , data: formData
                            , type: 'POST'
                            , success: function (data) {
                                if (editar) {
                                    window.open(controller + actionMontarTela + '/' + r.id, '_self');
                                } else {
                                    window.open(controller + actionMontarTela, '_self');
                                }
                            } //'../Exemplo/MontarTelaExemplo'
                            , error: function (data) { console.log('ERROR!!'); }
                            , cache: false
                            , processData: false
                            , contentType: false
                        });
                    } else {
                        if (editar && r.id != undefined) {
                            window.open(controller + actionMontarTela + '/' + r.id, '_self');
                        } else if (r.error != undefined) {
                            $('.ibox-content').find('.alert').remove();
                            $('.ibox-content').prepend(
                                '<div class="alert alert-danger">'
                                + '<span>' + r.error + '</span>'
                                + '<button type="button" class="close" data-dismiss="alert">Fechar</button>'
                                + '</div>');
                        } else {
                            window.open(controller + actionMontarTela, '_self');
                        }
                    }
                }
                else {
                    if (r.error == undefined) {
                        var alert = $(r).find('.alert');
                        $('.ibox-content').find('.alert').remove();
                        $('.ibox-content').prepend(alert);
                    }
                    else {
                        $('.ibox-content').find('.alert').remove();
                        $('.ibox-content').prepend(
                            '<div class="alert alert-danger">'
                            + '<span>' + r.error + '</span>'
                            + '<button type="button" class="close" data-dismiss="alert">Fechar</button>'
                            + '</div>');
                    }
                }
            }
        });
    });
}