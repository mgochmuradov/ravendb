package raven.client;

import org.apache.commons.httpclient.HttpClient;
import org.apache.commons.httpclient.HttpStatus;
import org.apache.commons.httpclient.methods.DeleteMethod;
import org.apache.commons.httpclient.methods.PutMethod;
import org.apache.commons.httpclient.methods.StringRequestEntity;
import org.junit.Test;

import raven.client.json.RavenJObject;
import raven.client.json.RavenJValue;
import raven.client.utils.UrlUtils;

public class RavenDBAwareTests {

  public final static String DEFAULT_SERVER_URL = "http://localhost:8123";

  private HttpClient client = new HttpClient();

  public String getServerUrl() {
    return DEFAULT_SERVER_URL;
  }

  public void createDb(String dbName) throws Exception {
    PutMethod put = null;
    try {
      put = new PutMethod(getServerUrl() + "/admin/databases/" + UrlUtils.escapeDataString(dbName));
      put.setRequestEntity(new StringRequestEntity(getCreateDbDocument(dbName), "application/json", "utf-8"));
      int statusCode = client.executeMethod(put);
      if (statusCode != HttpStatus.SC_OK) {
        throw new IllegalStateException("Invalid response on put:" + statusCode);
      }
    } finally {
      if (put != null) {
        put.releaseConnection();
      }
    }
  }

  private String getCreateDbDocument(String dbName) {
    RavenJObject doc = new RavenJObject();
    RavenJObject settings = new RavenJObject();
    doc.add("Settings", settings);
    settings.add("Raven/DataDir", RavenJValue.fromObject("~\\Databases\\" + dbName));
    settings.add("Raven/ActiveBundles", RavenJValue.fromObject("Raven/ActiveBundles"));
    doc.add("SecuredSettings", new RavenJObject());
    doc.add("Disabled", new RavenJValue(false));
    return doc.toString();
  }

  @Test
  public void testCreateDb() throws Exception {
    createDb("db1");
    deleteDb("db1");
  }

  private void deleteDb(String dbName) throws Exception {

    DeleteMethod deleteMethod = null;
    try {
      deleteMethod = new DeleteMethod(getServerUrl() + "/admin/databases/" + UrlUtils.escapeDataString(dbName));
      deleteMethod.setQueryString("hard-delete=true");
      int statusCode = client.executeMethod(deleteMethod);
      if (statusCode != HttpStatus.SC_OK) {
        throw new IllegalStateException("Invalid response on put:" + statusCode);
      }
    } finally {
      if (deleteMethod != null) {
        deleteMethod.releaseConnection();
      }
    }

  }
}
