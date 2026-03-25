export type OrderStatus = 'Created' | 'Processing' | 'Sent'

export interface OrderItem {
  productId: number
  quantity: number
  value: number
}

export interface Order {
  id: number
  orderId: number
  clientId: number
  tax: number
  status: OrderStatus
  items: OrderItem[]
}

export interface CreateOrderResponse {
  id: number
  status: string
}
