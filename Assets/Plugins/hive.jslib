mergeInto(LibraryManager.library, {

	Connect: function (username) {
		var user = UTF8ToString(username);
		if (window.hive_keychain == null){
			return "not_installed";
		}
		window.hive_keychain.requestHandshake(function() {
			console.log("User: " + user);
			window.hive_keychain.requestSignBuffer(user, 'login', 'Posting', function(response) {
				console.log("Login received!");
				console.log(response);
				if (response['success'] == true) {
					console.log("Login successful, username: " + response['data']['username']);
					SendMessage("Launcher", "LoginCallback", "");
                    return "";
                }
				else {
					console.log("Login failed!");
					SendMessage("Launcher", "LoginCallback", "login_failed");
				}
			});
		});
	},


	//TEST CODE
	// requestSignTx
	SignTx: function (username, id, data, prompt) {
		window.hive_keychain.requestCustomJson(UTF8ToString(username), UTF8ToString(id), 'Posting', UTF8ToString(data), UTF8ToString(prompt), function(response) {
			console.log(response);
			if (response['success'] == true) {
				console.log("Transaction successful!");
			}
			else {
				console.log("Transaction failed!");
			}
		});
	},

	///transfer tokens
	Transfer: function (username, to, amount, memo) {
        window.hive_keychain.requestTransfer(
            UTF8ToString(username),
            UTF8ToString(to),
            parseFloat(UTF8ToString(amount)).toFixed(3).toString(),
            UTF8ToString(memo),
            'HIVE',
            (response) => {
				if (response["success"])
				{
					SendMessage("Launcher", "ConnectCallback", "true");
				}
				else
				{
					SendMessage("Launcher", "ConnectCallback", response["message"]);
				}
            },
            true,
        )

	}



});

