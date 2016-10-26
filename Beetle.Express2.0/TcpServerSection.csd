<?xml version="1.0" encoding="utf-8"?>
<configurationSectionModel xmlns:dm0="http://schemas.microsoft.com/VisualStudio/2008/DslTools/Core" dslVersion="1.0.0.0" Id="7e933924-a99d-44dd-972a-74e89396813f" namespace="Beetle.Express" xmlSchemaNamespace="urn:Beetle.Express" assemblyName="Beetle.Express" xmlns="http://schemas.microsoft.com/dsltools/ConfigurationSectionDesigner">
  <typeDefinitions>
    <externalType name="String" namespace="System" />
    <externalType name="Boolean" namespace="System" />
    <externalType name="Int32" namespace="System" />
    <externalType name="Int64" namespace="System" />
    <externalType name="Single" namespace="System" />
    <externalType name="Double" namespace="System" />
    <externalType name="DateTime" namespace="System" />
    <externalType name="TimeSpan" namespace="System" />
  </typeDefinitions>
  <configurationElements>
    <configurationSection name="ServerSection" codeGenOptions="Singleton, XmlnsProperty" xmlSectionName="serverSection">
      <elementProperties>
        <elementProperty name="Listens" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="listens" isReadOnly="false">
          <type>
            <configurationElementCollectionMoniker name="/7e933924-a99d-44dd-972a-74e89396813f/ListenCollection" />
          </type>
        </elementProperty>
      </elementProperties>
    </configurationSection>
    <configurationElement name="Listen">
      <attributeProperties>
        <attributeProperty name="Name" isRequired="true" isKey="true" isDefaultCollection="false" xmlName="name" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/7e933924-a99d-44dd-972a-74e89396813f/String" />
          </type>
        </attributeProperty>
        <attributeProperty name="Port" isRequired="true" isKey="false" isDefaultCollection="false" xmlName="port" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/7e933924-a99d-44dd-972a-74e89396813f/Int32" />
          </type>
        </attributeProperty>
        <attributeProperty name="Host" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="host" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/7e933924-a99d-44dd-972a-74e89396813f/String" />
          </type>
        </attributeProperty>
        <attributeProperty name="SendBufferSize" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="sendBufferSize" isReadOnly="false" defaultValue="4096">
          <type>
            <externalTypeMoniker name="/7e933924-a99d-44dd-972a-74e89396813f/Int32" />
          </type>
        </attributeProperty>
        <attributeProperty name="ReceiveBufferSize" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="receiveBufferSize" isReadOnly="false" defaultValue="4096">
          <type>
            <externalTypeMoniker name="/7e933924-a99d-44dd-972a-74e89396813f/Int32" />
          </type>
        </attributeProperty>
        <attributeProperty name="ReceiveDataPoolSize" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="receiveDataPoolSize" isReadOnly="false" defaultValue="1000">
          <type>
            <externalTypeMoniker name="/7e933924-a99d-44dd-972a-74e89396813f/Int32" />
          </type>
        </attributeProperty>
        <attributeProperty name="Handler" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="handler" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/7e933924-a99d-44dd-972a-74e89396813f/String" />
          </type>
        </attributeProperty>
        <attributeProperty name="Type" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="type" isReadOnly="false" defaultValue="&quot;TCP&quot;">
          <type>
            <externalTypeMoniker name="/7e933924-a99d-44dd-972a-74e89396813f/String" />
          </type>
        </attributeProperty>
        <attributeProperty name="Dispatchs" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="dispatchs" isReadOnly="false" defaultValue="3">
          <type>
            <externalTypeMoniker name="/7e933924-a99d-44dd-972a-74e89396813f/Int32" />
          </type>
        </attributeProperty>
      </attributeProperties>
    </configurationElement>
    <configurationElementCollection name="ListenCollection" collectionType="AddRemoveClearMap" xmlItemName="serverListen" codeGenOptions="Indexer, AddMethod, RemoveMethod, GetItemMethods">
      <itemType>
        <configurationElementMoniker name="/7e933924-a99d-44dd-972a-74e89396813f/Listen" />
      </itemType>
    </configurationElementCollection>
  </configurationElements>
  <propertyValidators>
    <validators />
  </propertyValidators>
</configurationSectionModel>