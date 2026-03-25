import { useState } from 'react'
import { OrderList } from '../components/OrderList'
import { OrderDetail } from '../components/OrderDetail'
import { CreateOrderForm } from '../components/CreateOrderForm'
import type { OrderStatus } from '../types/order'
import { Plus, X } from 'lucide-react'

const tabs: { label: string; value: OrderStatus }[] = [
  { label: 'Criados', value: 'Created' },
  { label: 'Processando', value: 'Processing' },
  { label: 'Enviados', value: 'Sent' },
]

export function OrdersPage() {
  const [activeTab, setActiveTab] = useState<OrderStatus>('Created')
  const [selectedOrderId, setSelectedOrderId] = useState<number | null>(null)
  const [showCreate, setShowCreate] = useState(false)

  const handleCreated = (id: number) => {
    setShowCreate(false)
    setSelectedOrderId(id)
  }

  if (selectedOrderId !== null) {
    return (
      <OrderDetail
        orderId={selectedOrderId}
        onBack={() => setSelectedOrderId(null)}
      />
    )
  }

  return (
    <div className="space-y-5">
      <div className="flex items-center justify-between">
        <h1 className="text-xl font-semibold text-gray-800">Pedidos</h1>
        <button
          onClick={() => setShowCreate((v) => !v)}
          className="flex items-center gap-1.5 rounded bg-violet-600 px-3 py-2 text-sm font-medium text-white hover:bg-violet-700"
        >
          {showCreate ? <><X size={14} /> Cancelar</> : <><Plus size={14} /> Novo Pedido</>}
        </button>
      </div>

      {showCreate && (
        <div className="rounded-lg border border-gray-200 p-5">
          <h2 className="mb-4 text-sm font-semibold text-gray-700">Novo Pedido</h2>
          <CreateOrderForm onSuccess={handleCreated} />
        </div>
      )}

      <div className="border-b border-gray-200">
        <div className="flex gap-1">
          {tabs.map((tab) => (
            <button
              key={tab.value}
              onClick={() => setActiveTab(tab.value)}
              className={`px-4 py-2 text-sm font-medium transition-colors ${
                activeTab === tab.value
                  ? 'border-b-2 border-violet-600 text-violet-600'
                  : 'text-gray-500 hover:text-gray-800'
              }`}
            >
              {tab.label}
            </button>
          ))}
        </div>
      </div>

      <OrderList status={activeTab} onSelectOrder={setSelectedOrderId} />
    </div>
  )
}
