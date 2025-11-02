async function getReleases() {
	const response = await fetch('https://api.github.com/repos/voltstro/pootis-bot/releases');
	return response.json();
}

function getAllReleases() {
	getReleases().then((data) => {
		for(i=0;i<data.length;i++) {
			var preReleaseText = "";

			if(data[i].prerelease == true) {
				preReleaseText+= " <span class='pre_release'>Pre-release</span>";
			}

			var title = document.createElement("h2");
			title.innerHTML = "<a href='" + data[i].html_url + "'>" + data[i].name + "</a>" + preReleaseText;

			var details = document.createElement("p");
			details.innerHTML = data[i].tag_name +  " - Uploaded at: " + data[i].published_at;

			var element = document.getElementById("content");    
			element.appendChild(title);
			element.appendChild(details);

			var downloadsText = "Downloads:<br>";

			for(d=0;d<data[i].assets.length;d++) {
				
				if(downloadsText != "Downloads:<br>") {
					downloadsText+= " - ";
				}
				
				downloadsText+= "<a href='" + data[i].assets[d].browser_download_url + "'>" + data[i].assets[d].name + "</a> ";
			}

			var download = document.createElement("p");
			download.innerHTML = downloadsText;

			element.appendChild(download);
		}
	});
}

function getLatestRelease() {
	getReleases().then((data) => {
		var preReleaseText = "";

        if(data[0].prerelease == true) {
            preReleaseText+= " <span class='pre_release'>Pre-release</span>";
        }

		var title = document.createElement("h2");
		title.innerHTML = "Latest Version: <a href='" + data[0].html_url + "'>" + data[0].name + "</a>" + preReleaseText;

		var details = document.createElement("p");
		details.innerHTML = data[0].tag_name +  " - Uploaded at: " + data[0].published_at;

		var element = document.getElementById("latestText");    
		element.appendChild(title);
		element.appendChild(details);

		var downloadsText = "Downloads:<br>";

		for(d=0;d<data[0].assets.length;d++) {
				
			if(downloadsText != "Downloads:<br>") {
				downloadsText+= " - ";
			}
				
			downloadsText+= "<a href='" + data[0].assets[d].browser_download_url + "'>" + data[0].assets[d].name + "</a> ";
		}

		var download = document.createElement("p");
		download.innerHTML = downloadsText;

		element.appendChild(download);
	});
}